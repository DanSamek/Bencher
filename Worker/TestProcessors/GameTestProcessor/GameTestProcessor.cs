using System.Diagnostics;
using System.Text.RegularExpressions;
using Shared.Dtos.Requests;
using Shared.Dtos.Responses;
using Worker.ProcessOperations;
using Worker.UI;

namespace Worker.TestProcessors.GameTestProcessor;

/// <summary>
/// Game test processor.
/// - Plays games using fastchess.
/// </summary>
public class GameTestProcessor : ITestProcessor<GameTestProcessorResult>
{
    private readonly Communication.Communication _communication;
    private readonly Notifier.Notifier _notifier;
    private readonly ErrorTrace _errorTrace;
    private readonly GetTestNonAutobenchResponse _getTestNonAutobenchResponse;
    private readonly int _availableThreads;
    private readonly int _pairsNeeded;
    private readonly int _connectionId;
    private readonly CommonProcesses _commonProcesses;
    private readonly TestStateWatcher _testStateWatcher;
    
    private readonly Lock _lock = new Lock();
    
    private const byte WDL_W = 1;
    private const byte WDL_D = 2;   
    private const byte WDL_L = 4;
    
    private static IReadOnlyList<(Regex, byte)> _fastChessResults = new List<(Regex, byte)>
    {
        // Draws
        (new Regex("game (\\d+) \\(new vs base\\): 1\\/2-1\\/2"), WDL_D),
        (new Regex("game (\\d+) \\(base vs new\\): 1\\/2-1\\/2"), WDL_D),
        
        // Wins [dev perspective]
        (new Regex("game (\\d+) \\(new vs base\\): 1-0"), WDL_W),
        (new Regex("game (\\d+) \\(base vs new\\): 0-1"), WDL_W),
        
        // Loses [dev perspective]
        (new Regex("game (\\d+) \\(new vs base\\): 0-1"), WDL_L),
        (new Regex("game (\\d+) \\(base vs new\\): 1-0"), WDL_L)
    };
    
    private record PrepareEngineArguments(string GitUrl, string Branch, byte[] BuildScript, ErrorTrace ErrorTrace);

    /// <summary>
    /// .Ctor
    /// </summary>
    public GameTestProcessor(Communication.Communication communication, ErrorTrace errorTrace, GetTestNonAutobenchResponse getTestNonAutobenchResponse, int availableThreads, Notifier.Notifier notifier, CommonProcesses commonProcesses, TestStateWatcher testStateWatcher)
    {
        _communication = communication;
        _errorTrace = errorTrace;
        _getTestNonAutobenchResponse = getTestNonAutobenchResponse;
        _availableThreads = availableThreads;
        _notifier = notifier;
        _commonProcesses = commonProcesses;
        _pairsNeeded = GamePairCalculator.CalculatePairsNeeded(getTestNonAutobenchResponse.TimeManagement, _getTestNonAutobenchResponse.NumberOfThreads, availableThreads); // This is not optimal to calculate here, but who cares.
        _connectionId = _getTestNonAutobenchResponse.ConnectionId;
        _testStateWatcher = testStateWatcher;
    }
    
    /// <inheritdoc /> 
    public async Task<GameTestProcessorResult> Process()
    {
        // Builds engines.
        var (baseDirectory, newDirectory) = BuildEngines();
        // Create opening book [from the response].
        var openingBookPath = await CreateOpeningBook();
        
        if (_errorTrace.Error()) return GameTestProcessorResult.Error;
        if (!_notifier.IsTestStillRunning(_connectionId)) return ReturnNotRunningClean();
        
        // Run benches.
        _errorTrace.AddInfo("Running engine benches");
        var baseNps = ValidateBenches(baseDirectory, newDirectory);
        if (_errorTrace.Error()) return GameTestProcessorResult.Error;
        if (!_notifier.IsTestStillRunning(_connectionId)) return ReturnNotRunningClean();  
        
        var arguments = FastchessArgumentsCreator.CreateArguments(openingBookPath, baseDirectory, newDirectory, baseNps, _getTestNonAutobenchResponse, _availableThreads);
        
        if (!_notifier.IsTestStillRunning(_connectionId)) return ReturnNotRunningClean();
        
        // Run games.
        RunGames(arguments);
        
        CleanTmp(baseDirectory, newDirectory, openingBookPath);
        return await Task.FromResult(GameTestProcessorResult.Success);

        GameTestProcessorResult ReturnNotRunningClean()
        {
            CleanTmp(baseDirectory, newDirectory, openingBookPath);
            return GameTestProcessorResult.NotRunning;
        }
    }
    
    private void RunGames(string arguments)
    {
        _errorTrace.AddInfo($"Running games with arguments: {arguments}");
        var process = _commonProcesses.RunFastchess(arguments);
        if (process is null)
        {
            _errorTrace.AddError("Unable to run fastchess");
            return;
        }

        Task.Run(() => _testStateWatcher.Watch());
        
        var pairResults = new Dictionary<int, List<byte>>();
        var gamesEnded = 0;
        process.OutputDataReceived += ((_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            
            bool running;
            lock(_lock)
            {
                ProcessLine(e.Data);
                var (resultsDto, canSend, toRemove) = PrepareResults(pairResults);
                running = SendPairResults(resultsDto, canSend);
                if (canSend) toRemove.ForEach(k => pairResults.Remove(k));
            }
           
             // Stop fastchess, if test is not running or something happened with the communication (network problems for example).
             if (!_testStateWatcher.Running || !running || _communication.Error()) process.Kill();
        });
        
        process.BeginOutputReadLine();
        // TODO process.BeginErrorReadLine();
        
        process.WaitForExit();
        process.Close();
        
        var (results, _, _) = PrepareResults(pairResults);
        SendPairResults(results, true);
        
        _testStateWatcher.EnsureStopped();
        if (!_testStateWatcher.Running) _errorTrace.AddInfo("Test was manually stopped");
        
        return;
        void ProcessLine(string line)
        {
            var matched = false;
            foreach (var (regex, wdl) in _fastChessResults)
            {
                var match = regex.Match(line);
                if (!match.Success) continue;

                var bucket = (int.Parse(match.Groups[1].Value) - 1) / 2;

                if (pairResults.TryGetValue(bucket, out var value)) value.Add(wdl);
                else pairResults.Add(bucket, [wdl]);

                gamesEnded++;
                matched = true;
                break;
            }

            if (!matched)
            {
                _errorTrace.AddInfo($"Unknown line: {line}");
                return;
            }

            if (gamesEnded % _pairsNeeded == 0)
            {
                _errorTrace.AddInfo($"Games ended: {gamesEnded}");
            }
        }
    }

    private record PreparedResults(ResultsDto Results, bool CanSend, List<int> ToRemove);
    private PreparedResults PrepareResults(Dictionary<int, List<byte>> pairResults)
    {
        var results = new ResultsDto
        {
            Ll = 0,
            Ld = 0,
            Dd = 0,
            Wl = 0,
            Wd = 0,
            Ww = 0,
            ConnectionId = _connectionId
        };
        
        var currentPairsDone = 0;
        var toRemove = new List<int>();
        
        foreach (var (key, data) in pairResults)
        {
            Debug.Assert(data.Count <= 2);
            if (data.Count != 2) continue;
            
            if (data.All(x => x == WDL_L)) results.Ll++;
            if (data.All(x => x == WDL_W)) results.Ww++;
            if (data.All(x => x == WDL_D)) results.Dd++;
            
            var sum = data[0] + data[1];
            if (sum == WDL_L + WDL_D) results.Ld++;
            if (sum == WDL_W + WDL_L) results.Wl++;
            if (sum == WDL_W + WDL_D) results.Wd++;
            
            toRemove.Add(key);
            currentPairsDone++;
        }
        return new (results, currentPairsDone >= _pairsNeeded, toRemove);
    }
    
    private bool SendPairResults(ResultsDto results, bool canSend)
    {
        if (!canSend) return true; 
        
        var result = _communication.Results(results);
        return result?.Running ?? false;
    }
    
    private int ValidateBenches(DirectoryInfo baseDirectory,
                                DirectoryInfo newDirectory)
    { 
        var baseBench = _commonProcesses.RunBench(baseDirectory.FullName, _errorTrace);
        var testBench = _commonProcesses.RunBench(newDirectory.FullName, _errorTrace);
        
        AddErrorIfFalse(baseBench.Bench == _getTestNonAutobenchResponse.BaseBranchBench, 
            "Base bench differ from the entered bench");
        
        AddErrorIfFalse(testBench.Bench == _getTestNonAutobenchResponse.TestBranchBench, 
            "Test bench differ from the entered bench");

        return baseBench.Nps;
        void AddErrorIfFalse(bool expression, string error)
        {
            if (!expression)
            {
                _errorTrace.AddError(error);
            }
        }
    }
    
    private DirectoryInfo PrepareEngine(PrepareEngineArguments args)
    {
        var errorTrace = args.ErrorTrace;
        var gitUrl = args.GitUrl;
        var branch = args.Branch;
        var buildScript = args.BuildScript;
        errorTrace.AddInfo("Preparing engine");
    
        var directory = Directory.CreateTempSubdirectory();
    
        errorTrace.AddInfo($"Cloning repository - {gitUrl} - branch {branch}");
        _commonProcesses.CloneRepository(gitUrl, branch, directory.FullName, errorTrace);
        if (errorTrace.Error()) return directory;
    
        errorTrace.AddInfo("Building engine");
        _commonProcesses.Build(buildScript, directory.FullName, errorTrace); 
        return directory;
    }

    private async Task<string> CreateOpeningBook()
    {
        var openingBook = _getTestNonAutobenchResponse.OpeningBook;
        var openingBookDirectory = Directory.CreateTempSubdirectory();
        var openingBookFilePath = $"{openingBookDirectory.FullName}/{openingBook.Name}";
        await using var fs = new FileStream(openingBookFilePath, FileMode.OpenOrCreate);
        await fs.WriteAsync(openingBook.Data);
        return openingBookFilePath;
    }

    private (DirectoryInfo BaseDirectory, DirectoryInfo NewDirectory) BuildEngines()
    {
        _errorTrace.AddInfo("Cloning and building engines");
        var args = new PrepareEngineArguments(_getTestNonAutobenchResponse.GitUrl, _getTestNonAutobenchResponse.BaseBranch,
            _getTestNonAutobenchResponse.BuildScript!, _errorTrace);
        var baseDirectory = PrepareEngine(args);
        args = args with { Branch = _getTestNonAutobenchResponse.TestBranch };
        var newDirectory = PrepareEngine(args);
        return (baseDirectory, newDirectory);
    }
    
    private static void CleanTmp(DirectoryInfo baseDirectory, DirectoryInfo newDirectory, string openingBookPath)
    {
        if (Directory.Exists(baseDirectory.FullName)) Directory.Delete(baseDirectory.FullName, true);
        if (Directory.Exists(newDirectory.FullName)) Directory.Delete(newDirectory.FullName, true);
        if (File.Exists(openingBookPath)) File.Delete(openingBookPath);
    }
}
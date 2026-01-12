using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Shared.Dtos.Requests;
using Shared.Dtos.Responses;
using Worker.Dependencies;

namespace Worker.TestProcessors;

/// <summary>
/// Game test processor.
/// - Plays games via fastchess.
/// </summary>
public class GameTestProcessor : ITestProcessor<bool>
{
    private readonly Communication _communication;
    private readonly ErrorTrace _errorTrace;
    private readonly GetTestNonAutobenchResponse _getTestNonAutobenchResponse;
    private readonly int _processorThreads;
    private readonly int _pairsNeeded;
    
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
    public GameTestProcessor(Communication communication, ErrorTrace errorTrace, GetTestNonAutobenchResponse getTestNonAutobenchResponse, int processorThreads)
    {
        _communication = communication;
        _errorTrace = errorTrace;
        _getTestNonAutobenchResponse = getTestNonAutobenchResponse;
        _processorThreads = processorThreads;
        _pairsNeeded = CalculatePairsNeeded(getTestNonAutobenchResponse);
    }
    
    /// <inheritdoc /> 
    public async Task<bool> Process()
    {
        // Builds engines.
        var (baseDirectory, newDirectory) = BuildEngines();
        if (_errorTrace.Error()) return false;
        
        // Run benches.
        _errorTrace.AddInfo("Running engine benches");
        var baseNps = ValidateBenches(baseDirectory, newDirectory);
        if (_errorTrace.Error()) return false;
        
        // Create opening book [from the response].
        var openingBookPath = await CreateOpeningBook();
        var arguments = CreateFastchessArguments(openingBookPath, baseDirectory, newDirectory, baseNps);
        
        // Run games.
        RunGames(arguments);
        
        CleanTmp(baseDirectory, newDirectory, openingBookPath);
        return await Task.FromResult(true);
    }
    
    private void RunGames(string arguments)
    {
        _errorTrace.AddInfo($"Running games with arguments: {arguments}");
        var processStartInfo = Helper.CreateProcessStartInfo(arguments, $"{FastchessDependency.FASTCHESS_BINARY_PATH}/fastchess");
        var process = System.Diagnostics.Process.Start(processStartInfo);

        if (process is null)
        {
            _errorTrace.AddError("Unable to run fastchess");
            return;
        }
        
        var pairResults = new Dictionary<int, List<byte>>();
        process.OutputDataReceived += ((_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            //#if DEBUG
                Console.WriteLine(e.Data);
           // #endif
            var hit = false;
            foreach (var (regex, wdl) in _fastChessResults)
            {
                var match = regex.Match(e.Data);
                if (!match.Success) continue;
                
                var bucket = (int.Parse(match.Groups[1].Value) - 1) / 2;
                
                if (pairResults.TryGetValue(bucket, out var value)) value.Add(wdl);
                else pairResults.Add(bucket, [wdl]);

                hit = true;
                break;
            }
            if (!hit) Console.WriteLine("Unknown line: " + e.Data);
            
            var running = SendPairResults(pairResults, false);
            if (!running) process.Kill(); // Stop fastchess, if test is not running.
        });
        
        process.BeginOutputReadLine();
        // TODO process.BeginErrorReadLine();
        
        process.WaitForExit();
        process.Close();
        SendPairResults(pairResults, true);
    }

    private bool SendPairResults(Dictionary<int, List<byte>> pairResults, bool force)
    {
        var results = new ResultsDto
        {
            Ll = 0,
            Ld = 0,
            Dd = 0,
            Wl = 0,
            Wd = 0,
            Ww = 0,
            ConnectionId = _getTestNonAutobenchResponse.ConnectionId
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

        if (currentPairsDone < _pairsNeeded && !force) return true; 
        
        var result = _communication.Results(results);
        toRemove.ForEach(k => pairResults.Remove(k));
        return result?.Running ?? false;
    }
    
    
    private int ValidateBenches(DirectoryInfo baseDirectory,
                                DirectoryInfo newDirectory)
    { 
        var baseBench = ProcessorHelper.RunBench(baseDirectory.FullName, _errorTrace);
        var testBench = ProcessorHelper.RunBench(newDirectory.FullName, _errorTrace);
        
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
    
    private static DirectoryInfo PrepareEngine(PrepareEngineArguments args)
    {
        var errorTrace = args.ErrorTrace;
        var gitUrl = args.GitUrl;
        var branch = args.Branch;
        var buildScript = args.BuildScript;
        errorTrace.AddInfo("Preparing engine");
    
        var directory = Directory.CreateTempSubdirectory();
    
        errorTrace.AddInfo($"Cloning repository - {gitUrl} - branch {branch}");
        ProcessorHelper.CloneRepository(gitUrl, branch, directory.FullName, errorTrace);
        if (errorTrace.Error()) return directory;
    
        errorTrace.AddInfo("Building engine");
        ProcessorHelper.Build(buildScript, directory.FullName, errorTrace); 
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
    
    /*
        -engine proto=uci cmd="./stockfish-dev" name="stockfish-dev"
        -engine proto=uci cmd="./stockfish" name="stockfish"
        -each tc=0:08+0.08
        -rounds 100000  [N / 2]
        -games 2
        -repeat
        -concurrency 8   ProcessorThreads / _getTestNonAutobenchResponse.NumberOfThreads
        -ratinginterval 0
        -openings file="./openings/UHO_4060_v2.epd" [from the response] format=epd [from the response]. order=random
        -recover
        -scoreinterval 0
        -resign movecount=3 score=500 [additional fastchess options]
     */
    private string CreateFastchessArguments(string openingBookPath, 
                                            DirectoryInfo baseDirectory, 
                                            DirectoryInfo newDirectory,
                                            int baseNps)
    {
        var sb = new StringBuilder();
        var baseBinaryPath = Helper.EngineBinary(baseDirectory);
        var newBinaryPath = Helper.EngineBinary(newDirectory);

        sb.AddEngine($"cmd={newBinaryPath} name=new");
        sb.AddEngine($"cmd={baseBinaryPath} name=base");
        sb.AddArgument("-each");
        
        var (seconds, increment) = ScaleTc(baseNps);
        sb.AddArgument($"tc={seconds:F}+{increment:F}"); 
        sb.AddArgument($"option.Hash={_getTestNonAutobenchResponse.HashSize}");
        sb.AddArgument($"option.Threads={_getTestNonAutobenchResponse.NumberOfThreads}");
        
        sb.AddArgument($"-rounds {_getTestNonAutobenchResponse.NumberOfGames / 2}");
        sb.AddArgument("-games 2");
        sb.AddArgument("-repeat");
        
        sb.AddArgument($"-concurrency {_processorThreads / _getTestNonAutobenchResponse.NumberOfThreads}");
        sb.AddArgument("-ratinginterval 0");
        
        sb.AddOpeningBook(openingBookPath, _getTestNonAutobenchResponse.OpeningBook);
        
        sb.AddArgument("-recover");
        sb.AddArgument("-scoreinterval 0");
        
        if (_getTestNonAutobenchResponse.AdditionalFastchessOptions is not null)
        {
            sb.AddArgument(_getTestNonAutobenchResponse.AdditionalFastchessOptions);
        }
        
        var result = sb.ToString();
        return result;
    }
    
    private (decimal Seconds, decimal Increment) ScaleTc(int baseNps)
    {
        var timeScale = _getTestNonAutobenchResponse.ExpectedNps * (decimal)1.0 / baseNps;
        var timeManagement = _getTestNonAutobenchResponse.TimeManagement;
        (decimal seconds, decimal increment) = timeManagement.Tc();
        seconds *= timeScale;
        increment *= timeScale;
        return (seconds, increment);
    }
    
    private static void CleanTmp(DirectoryInfo baseDirectory, DirectoryInfo newDirectory, string openingBookPath)
    {
        Directory.Delete(baseDirectory.FullName, true);
        Directory.Delete(newDirectory.FullName, true);
        File.Delete(openingBookPath);
    }
    
    private int CalculatePairsNeeded(GetTestNonAutobenchResponse nonAutobenchResponse)
    {
        var (seconds, _) = nonAutobenchResponse.TimeManagement.Tc();
        var numberOfThreads = nonAutobenchResponse.NumberOfThreads;
        
        var log = (int)Math.Log2(seconds * 1.0 / 30 + numberOfThreads);
        var perProcessor = Math.Max(4 - log, 1);
        return perProcessor * (_processorThreads / numberOfThreads);
    }
}

file static class Extensions
{
    public static void AddOpeningBook(this StringBuilder builder, string openingBookPath, OpeningBookDto openingBookDto)
    {
        var openingBookType = openingBookDto.OpeningBookType;
        builder.AddArgument($"-openings order=random file={openingBookPath} format={openingBookType.ToString().ToLower()}");
    }
    
    public static void AddEngine(this StringBuilder sb, string cmdName)
    {
        sb.AddArgument($"-engine proto=uci {cmdName}");
    }

    public static void AddArgument(this StringBuilder sb, string argument)
    {   
        sb.Append($"{argument} ");
    }
    
    public static (int Seconds, decimal Increment) Tc(this string s)
    {
        var plusIndex = s.IndexOf('+');
        if (plusIndex == -1) throw new Exception("String value is not in the expected format.");
        
        var split = s.Split("+");
        return (int.Parse(split[0]), decimal.Parse(split[1]));
    }
}

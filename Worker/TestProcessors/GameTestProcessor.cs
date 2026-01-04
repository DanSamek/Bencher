using Shared.Dtos.Responses;

namespace Worker.TestProcessors;

public record GameTestProcess(GetTestNonAutobenchResponse NonAutobenchResponse, ErrorTrace ErrorTrace);

/// <summary>
/// Game test processor.
/// - Plays games via fastchess.
/// </summary>
public class GameTestProcessor : ITestProcessor<GameTestProcess, bool> // TODO
{
    private readonly Communication _communication;

    private record PrepareEngineArguments(string GitUrl, string Branch, byte[] BuildScript, ErrorTrace ErrorTrace);
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public GameTestProcessor(Communication communication) => _communication = communication;
    
    /// <inheritdoc /> 
    public async Task<bool> Process(GameTestProcess gameTestProcess)
    {
        var nonAutoBenchResponse = gameTestProcess.NonAutobenchResponse;
        var errorTrace = gameTestProcess.ErrorTrace;
     
        // Builds engines.
        gameTestProcess.ErrorTrace.AddInfo("Cloning and building engines");
        var args = new PrepareEngineArguments(nonAutoBenchResponse.GitUrl, nonAutoBenchResponse.BaseBranch,
                                              nonAutoBenchResponse.BuildScript!, errorTrace);
        var baseDirectory = PrepareEngine(args);
        if (errorTrace.Error()) return false;
        args = args with { Branch = nonAutoBenchResponse.TestBranch };
        var newDirectory = PrepareEngine(args);
        if (errorTrace.Error()) return false;
        
        // Runs benches.
        gameTestProcess.ErrorTrace.AddInfo("Running engine benches");
        var baseBench = ProcessorHelper.RunBench(baseDirectory.Name, errorTrace);
        var testBench = ProcessorHelper.RunBench(newDirectory.Name, errorTrace);
        
        ValidateBenches(baseBench, testBench, gameTestProcess);
        if (errorTrace.Error()) return false;
        
        // TODO save to the /tmp/{randomdir} opening book.
        
        // Run games.
        /*
            -engine proto=uci cmd="./stockfish-dev" name="stockfish-dev" -engine cmd="./stockfish" proto=uci name="stockfish" -each tc=0:08+0.08 -randomseed -rounds 100000 -games 2 -repeat -concurrency 8 -ratinginterval 10 -openings file="./openings/UHO_4060_v2.epd" format=epd order=random -sprt elo0=0 elo1=5 alpha=0.05 beta=0.05 -recover -scoreinterval 0 -resign movecount=3 score=500
         */
        var arguments = $" {nonAutoBenchResponse.AdditionalFastchessOptions}";
        var processStartInfo = Helper.CreateProcessStartInfo(arguments, "/tmp/bencher-bin/fastchess");
        Helper.RunProcess(processStartInfo); // TODO we need instant output from the process not, only when ended.
        
        // TODO delete directories.
        return await Task.FromResult(false);
    }
    
    private void ValidateBenches(ProcessorHelper.BenchResult baseBench, 
                                 ProcessorHelper.BenchResult testBench,
                                 GameTestProcess gameTestProcess)
    {
        var errorTrace = gameTestProcess.ErrorTrace;
        if (baseBench.Bench != gameTestProcess.NonAutobenchResponse.BaseBranchBench)
        {
            errorTrace.AddError("Base bench differ from the entered bench");
        }

        if (testBench.Bench != gameTestProcess.NonAutobenchResponse.TestBranchBench)
        {
            errorTrace.AddError("Test bench differ from the entered bench");
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
}
using System.Text;
using Shared.Dtos.Responses;

namespace Worker.TestProcessors.GameTestProcessor;

public static class FastchessArgumentsCreator
{
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
    /// <summary>
    /// Creates arguments for the fastchess app.
    /// </summary>
    public static string CreateArguments(string openingBookPath, 
        DirectoryInfo baseDirectory, 
        DirectoryInfo newDirectory,
        int baseNps,
        GetTestNonAutobenchResponse  getTestNonAutobenchResponse,
        int processorThreads)
    {
        var sb = new StringBuilder();
        var baseBinaryPath = Helper.EngineBinary(baseDirectory);
        var newBinaryPath = Helper.EngineBinary(newDirectory);

        sb.AddEngine($"cmd={newBinaryPath} name=new");
        sb.AddEngine($"cmd={baseBinaryPath} name=base");
        sb.AddArgument("-each");
        
        var (seconds, increment) = TmScaler.Scale(baseNps, getTestNonAutobenchResponse.ExpectedNps, getTestNonAutobenchResponse.TimeManagement);
        sb.AddArgument($"tc={seconds:F3}+{increment:F3}"); 
        sb.AddArgument($"option.Hash={getTestNonAutobenchResponse.HashSize}");
        sb.AddArgument($"option.Threads={getTestNonAutobenchResponse.NumberOfThreads}");
        
        sb.AddArgument($"-rounds {getTestNonAutobenchResponse.NumberOfGames / 2}");
        sb.AddArgument("-games 2");
        //sb.AddArgument("-repeat");
        
        sb.AddArgument($"-concurrency {processorThreads / getTestNonAutobenchResponse.NumberOfThreads}");
        sb.AddArgument("-ratinginterval 0");
        
        sb.AddOpeningBook(openingBookPath, getTestNonAutobenchResponse.OpeningBook);
        
        sb.AddArgument("-recover");
        sb.AddArgument("-scoreinterval 0");
        
        if (getTestNonAutobenchResponse.AdditionalFastchessOptions is not null)
        {
            sb.AddArgument(getTestNonAutobenchResponse.AdditionalFastchessOptions);
        }
        
        var result = sb.ToString();
        return result;
    }
}

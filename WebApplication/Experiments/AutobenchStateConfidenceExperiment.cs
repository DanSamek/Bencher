using WebApplication.API.Dtos.Requests;

namespace WebApplication.Experiments;

public class AutobenchStateConfidenceExperiment
{
    
    private const int BENCH = 1000;
    
    private static AutobenchDto CreateAutobenchDto(int id)
    {
        var result = new AutobenchDto
        {
            Autobench = BENCH,
            ConnectionId = 1
        };
        return result;
    }
    
    
    public static void Run()
    {
        for (var i = 1; i <= 9; i++)
        {
            var confidence = 1 / Math.Pow(2, i);
            RunIter(confidence);
        }
        
        RunIter(1 / 10.0);
        RunIter(1 / 25.0);
        RunIter(1 / 3.0);
        RunIter(1 / 5.0);
        return;
        
        static void RunIter(double confidence)
        {
            // Simulation of the /autobench
            var autobenchRequests = Enumerable
                .Range(0, 1000)
                .Select(CreateAutobenchDto)
                .ToArray();

            var state = new Data.Models.AutobenchState
            {
                Test = null!,
                TestId = 0,
                Confidence = 0,
                UserConfidence = confidence,
                Bench = BENCH
            };

            var expectedAutobenchResults = (int)Math.Round(1 / state.UserConfidence);
            Console.WriteLine(expectedAutobenchResults);
            for (var i = 0; i < autobenchRequests.Length; i++)
            {
                if (i >= expectedAutobenchResults) break;
         
                var result = state.UpdateConfidence(autobenchRequests[i].Autobench);
                System.Diagnostics.Debug.Assert(result);
            }
        
            Console.WriteLine(state.Confidence);
            System.Diagnostics.Debug.Assert(state.Resolved);
        }
    }
}
using WebApplication.Data.Models;

namespace WebApplication.Experiments;

public static class TestQueueExperiment
{
    private static double Scale(Test test, Func<double, double> fx)
       => fx(1.0 * test.ThreadScale) / test.ActiveWorkerThreadCount();
    
    private static Test CreateTest(int id, int numberOfThreads = 1, string timeManagement = "8+0.08")
    {
        var test = new Test
        {
            ExpectedNps = 1,
            WorkerLogs = [],
            Engine = null!,
            User = null!,
            Settings = null!,
            OpeningBook = null!,
            Errors = null!,
            Priority = 1,
            Id = id,
            Autobenched = false,
            ThreadScale = 0,
            Penta = null!,
            Name = $"Test {id}",
            AutobenchState = null,
            BaseBranch = null!,
            Created = DateTime.UtcNow,
            HashSize = 16,
            NumberOfThreads = numberOfThreads,
            State = TestState.Paused,
            TestBranch = null!,
            TimeManagement = timeManagement,
            BaseBranchId = 0,
            TestBranchId = 0
        };
        
        test.CalculateThreadScale();
        return test;
    }

    private static Test? TestWithoutWorkers(List<Test> tests) 
        => tests
            .OrderByDescending(t => t.ThreadScale)
            .FirstOrDefault(t => t.WorkerLogs.Count == 0);

    private static void Print(Test test)
    {
        Console.WriteLine($"Scale: {test.ThreadScale}, WorkerThreads: {test.ActiveWorkerThreadCount()}, WorkerThreadsEntries: [{string.Join(',', test.WorkerLogs.Select(x => x.NumberOfThreads))}]");
    }
    
    public static void Run()
    {
        var tests = new List<Test>();
        tests.Add(CreateTest(0, 4));
        tests.Add(CreateTest(1, 2));
        tests.Add(CreateTest(2, 1));

        var workerThreads = new[] { 8, 32, 16, 8, 4, 16, 4, 8, 16, 8, 8, 8, 8, 8, 16, 8, 16 };

        for (var iteration = 0; iteration < workerThreads.Length; iteration++)
        {
            var testToAddWorker = TestWithoutWorkers(tests) ?? tests.MaxBy(t => Scale(t, t => t / 2)); 
            
            testToAddWorker!.WorkerLogs.Add(
                new WorkerLog
                {
                    Name = "TEST WORKER",
                    Id = 0,
                    NumberOfThreads = workerThreads[iteration],
                    Mac = "",
                    NumberOfGames = 0,
                    TotalNumberOfGames = 64,
                    Test = testToAddWorker,
                    User = null!,
                    ConnectTime = DateTime.UtcNow,
                    State = WorkerLogState.Active
                });
            
            if (iteration % 5 != 0) continue;
            Console.WriteLine($"\niteration: {iteration}");
            tests.ForEach(Print);
        }
        
        tests.ForEach(Print);
    }
}
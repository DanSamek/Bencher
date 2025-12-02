using WebApplication.Data.Models;

namespace WebApplication.Experiments;

public static class TestQueueExperiment
{
    // ActiveWorkerThreadCount() has to be > 0.
    private static double Scale(Test test, Func<double, double> fx)
       => fx(1.0 * test.ThreadScale) / test.ActiveWorkerThreadCount();

    private static readonly string[] _timeManagements =
    [
        "8+0.08",
        "60+0.6",/*
        "120+1.2"*/
    ];
    
    private static Test CreateTest(int n)
    {
        var random = new Random(n);
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
            Id = n,
            Autobenched = false,
            ThreadScale = 0,
            Penta = null!,
            Name = $"Test {n}",
            AutobenchState = null,
            BaseBranch = null!,
            Created = DateTime.UtcNow,
            HashSize = 16,
            NumberOfThreads =1,// (int)Math.Pow(2, random.Next(1, 4)),
            State = TestState.Paused,
            TestBranch = null!,
            TimeManagement = _timeManagements[random.Next(0, _timeManagements.Length)],
            BaseBranchId = 0,
            TestBranchId = 0
        };
        
        test.CalculateThreadScale();
        return test;
    }

    private static Test? TestWithoutWorkers(List<Test> tests) => tests.OrderByDescending(t => t.ThreadScale).FirstOrDefault(t => t.WorkerLogs.Count == 0);

    private static void Print(Test test)
    {
        Console.WriteLine($"Scale: {test.ThreadScale}, WorkerThreads: {test.ActiveWorkerThreadCount()}, WorkerThreadsEntries: [{string.Join(',', test.WorkerLogs.Select(x => x.NumberOfThreads))}]");
    }
    public static void Run()
    {
        var tests = Enumerable
            .Range(0, 5)
            .Select(CreateTest)
            .ToList();
        
        // simple simulation of /get-test?autobench=[true|false]
        var workerThreads = Enumerable
            .Range(0, 200)
            .Select(_ => (int)Math.Pow(2, Random.Shared.Next(1, 4)))
            .ToList();

        for (var iteration = 0; iteration < workerThreads.Count; iteration++)
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
                    Errors = [],
                    State = WorkerLogState.Disconnected,
                    InitialTestState = InitialTestState.Normal
                });
            
            if (iteration % 5 != 0) continue;
            Console.WriteLine($"\niteration: {iteration}");
            tests.ForEach(Print);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using WebApplication.Data.Models;
using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class TestStoreTests : TestBase
{
    #region GetNextTestForWorker
    
    /// <summary>
    /// Test with 1 paused test,
    /// we expect, that this test will be picked by a store mechanism.
    /// </summary>
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void GetNextTestForWorker_NoRunningTest(bool requestAutobenched, bool shouldBeNull)
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch")
                .Close()
            .Close();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(requestAutobenched, 1);
        
        if (shouldBeNull)
        {
            Assert.That(nextTest, Is.Null);
        }
        else
        {
            Assert.That(nextTest, Is.Not.Null);
            Assert.That(nextTest.Name, Is.EqualTo("test_1"));
        }
    }
    
    /// <summary>
    /// Test with 1 paused autobenched test,
    /// we expect, that this test will be picked by a store mechanism.
    /// </summary>
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void GetNextTestForWorker_NoRunningTest_Autobenched(bool requestAutobenched, bool shouldBeNull)
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch")
            .AddAutobenchedTest("test_1", "test_book", "base_branch", "test_branch")
            .Close()
            .Close();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(requestAutobenched, 1);
        if (shouldBeNull)
        {
            Assert.That(nextTest, Is.Null);
        }
        else
        {
            Assert.That(nextTest, Is.Not.Null);
            Assert.That(nextTest.Name, Is.EqualTo("test_1"));
        }
    }
    
    /// <summary>
    /// Test with the multiple paused tests,
    /// we expect, that tests one by one will be returned from the method.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_AllTestsWithWorkers()
    {
        var domainBuilder = new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings();
        
        var testStore = new TestStore(Factory);
        
        for (var i = 0; i < 5; i++)
        {
            domainBuilder
                .CreateUser($"test_user{i}")
                    .AddEngine("stockfish")
                        .AddBranch("base_branch")
                        .AddBranch("test_branch")
                        .AddTest($"test_{i}", "test_book","base_branch",  "test_branch")
                    .Close()
                .Close();
        }
        
        Assert.That(5, Is.EqualTo(Factory.CreateDbContext().Tests.Count()));
        
        var nextTests = new HashSet<string>();
        for (var i = 0; i < 5; i++)
        {
            var result = testStore.GetNextTestForWorker(false, 1);
            Assert.That(result, Is.Not.Null);
            nextTests.Add(result.Name);
            
            var tmpStore = new TestStore(Factory);
            tmpStore.SetRunningState(result);
        }
        
        Assert.That(nextTests, Has.Count.EqualTo(5));
    }
    
    /// <summary>
    /// Test with the multiple paused tests,
    /// we expect, that tests one by one will be returned from the method.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_AllTestsWithWorkers_Autobenched()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .Close();

        var testStore = new TestStore(Factory);

        for (var i = 0; i < 5; i++)
        {
            new DomainBuilder(Factory.CreateDbContext())
                .CreateUser($"test_user{i}")
                    .AddEngine("stockfish")
                        .AddBranch("base_branch")
                        .AddBranch("test_branch")
                        .AddAutobenchedTest($"test_{i}", "test_book", "base_branch", "test_branch")
                            .Close()
                        .Close()
                    .Close()
                .Close();
        }

        Assert.That(5, Is.EqualTo(Factory.CreateDbContext().Tests.Count()));
        Assert.That(5, Is.EqualTo(Factory.CreateDbContext().AutobenchStates.Count()));

        var nextTests = new HashSet<string>();
        for (var i = 0; i < 5; i++)
        {
            var result = testStore.GetNextTestForWorker(true, 1);
            Assert.That(result, Is.Not.Null);
            nextTests.Add(result.Name);

            var tmpStore = new TestStore(Factory);
            tmpStore.SetRunningState(result);
        }

        Assert.That(nextTests, Has.Count.EqualTo(5));
    }

    /// <summary>
    /// Test with multiple stopped tests, but all of them requires more cores, than worker's cores.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_NotEnoughWorkerCores()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", numberOfThreads: 4)
                    .Close()        
                    .AddTest("test_2", "test_book", "base_branch", "test_branch", numberOfThreads: 2)
                    .Close()        
                    .AddTest("test_3", "test_book", "base_branch", "test_branch", numberOfThreads: 8)
                    .Close()
                .Close()
            .Close();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(false, 1);
        Assert.That(nextTest, Is.Null);
    }
    
    /// <summary>
    /// Test with multiple stopped tests, but all of them requires more cores, than worker's cores.
    /// Autobench variant.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_NotEnoughWorkerCores_Autobenched()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch")
            .Close()
            .Close()
            .Close();

        EngineBuilder.AddAutobenchedTestForUser("test_1", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), numberOfThreads: 2);
        
        EngineBuilder.AddAutobenchedTestForUser("test_2", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), numberOfThreads: 4);
        
        EngineBuilder.AddAutobenchedTestForUser("test_3", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), numberOfThreads: 8);
        
        Assert.That(3, Is.EqualTo(Factory.CreateDbContext().AutobenchStates.Count()));
        Assert.That(3, Is.EqualTo(Factory.CreateDbContext().Tests.Count(t => t.Autobenched)));
        Assert.That(3, Is.EqualTo(Factory.CreateDbContext().Tests.Include(t => t.AutobenchState).Count(t => t.AutobenchState != null)));

        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(true, 1);
        Assert.That(nextTest, Is.Null);
    }
    
    /// <summary>
    /// Test with multiple stopped tests, but all of them has different priorities.
    /// We expect, that test with the highest priority will be selected.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_BiggestPriority()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", priority: 0)
                    .Close()        
                    .AddTest("test_2", "test_book", "base_branch", "test_branch", priority: 1)
                    .Close()        
                    .AddTest("test_3", "test_book", "base_branch", "test_branch", priority: -1)
                .Close()
            .Close()
        .Close();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(false, 1);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_2"));
    }
    
    
    /// <summary>
    /// Test with multiple stopped tests, but all of them has different priorities.
    /// We expect, that test with the highest priority will be selected.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_BiggestPriority_Autobenched()
    {
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                .Close()
            .Close()
        .Close();
        
        
        EngineBuilder.AddAutobenchedTestForUser("test_1", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", factory.CreateDbContext(), priority: 0);
        
        EngineBuilder.AddAutobenchedTestForUser("test_2", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", factory.CreateDbContext(), priority: 1);
        
        EngineBuilder.AddAutobenchedTestForUser("test_3", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", factory.CreateDbContext(), priority: -1);
        
        Assert.That(3, Is.EqualTo(factory.CreateDbContext().AutobenchStates.Count()));
        Assert.That(3, Is.EqualTo(factory.CreateDbContext().Tests.Count(t => t.Autobenched)));
        Assert.That(3, Is.EqualTo(factory.CreateDbContext().Tests.Include(t => t.AutobenchState).Count(t => t.AutobenchState != null)));

        var testStore = new TestStore(factory);
        var nextTest = testStore.GetNextTestForWorker(true, 1);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_2"));
    }
    
    
    /// <summary>
    /// Test with multiple running tests, but all of them has different priorities.
    /// We expect, that test with the highest priority will be selected.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_MaxPriority_Running()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", priority: 0, state: TestState.Running)
                        .AddWorker(1)       
                    .Close()        
                    .AddTest("test_2", "test_book", "base_branch", "test_branch", priority: 1, state: TestState.Running)
                        .AddWorker(1)
                    .Close()        
                    .AddTest("test_3", "test_book", "base_branch", "test_branch", priority: -1, state: TestState.Running)
                        .AddWorker(1)
                    .Close()
                .Close()
            .Close()
        .Close();

        using var context = Factory.CreateDbContext();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(false, 1);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_2"));
    }
    
    
    /// <summary>
    /// Test with multiple autobenched tests, but all of them has different priorities.
    /// We expect, that test with the highest priority will be selected.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_MaxPriority_Running_Autobenched()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch");
          
        EngineBuilder.AddAutobenchedTestForUser("test_1", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 0,  state: TestState.Autobenched, workerThreads: [1]);

        EngineBuilder.AddAutobenchedTestForUser("test_2", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1, state: TestState.Autobenched, workerThreads: [1]);
        
        EngineBuilder.AddAutobenchedTestForUser("test_3", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: -1, state: TestState.Autobenched, workerThreads: [1]);

        using var context = Factory.CreateDbContext();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(true, 1);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_2"));
    }
    
    
    /// <summary>
    /// Test with multiple running tests and one paused (with same priority)
    /// We expect, that paused test will be selected.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_RunningSame_Priority_Paused_SamePriority()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", priority: 1, state: TestState.Running)
                        .AddWorker(1)       
                    .Close()        
                    .AddTest("test_2", "test_book", "base_branch", "test_branch", priority: 1, state: TestState.Running)
                        .AddWorker(1)
                    .Close()        
                    .AddTest("test_3", "test_book", "base_branch", "test_branch", priority: 1, state: TestState.Running)
                        .AddWorker(1)
                    .Close()
                    .AddTest("test_4", "test_book", "base_branch", "test_branch", priority: 1, state: TestState.Paused)
                    .Close()
                .Close()
            .Close()
        .Close();

        using var context = Factory.CreateDbContext();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(false, 1);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_4"));
    }
    
    
    /// <summary>
    /// Test with multiple autobenched tests and one paused (with same priority)
    /// We expect, that paused test will be selected.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_RunningSame_Priority_Paused_SamePriority_Autobenched()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch");

             
        EngineBuilder.AddAutobenchedTestForUser("test_1", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1,  state: TestState.Autobenched, workerThreads: [1]);

        EngineBuilder.AddAutobenchedTestForUser("test_2", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1, state: TestState.Autobenched, workerThreads: [1]);
        
        EngineBuilder.AddAutobenchedTestForUser("test_3", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1, state: TestState.Autobenched, workerThreads: [1]);
        
        EngineBuilder.AddAutobenchedTestForUser("test_4", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1);

        
        using var context = Factory.CreateDbContext();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(true, 1);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_4"));
    }
    
    /// <summary>
    /// Test with multiple running tests with a same priority, same number of worker threads, but different time managements.
    /// We expect, that test with the biggest time management will be returned.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_Running_DifferentTimeManagements()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Running, timeManagement: "8+0.08")
                        .AddWorker(1)       
                    .Close()        
                    .AddTest("test_2", "test_book", "base_branch", "test_branch", state: TestState.Running, timeManagement: "60+0.6")
                        .AddWorker(1)
                    .Close()        
                    .AddTest("test_3", "test_book", "base_branch", "test_branch", state: TestState.Running, timeManagement: "120+1.2")
                        .AddWorker(1)
                    .Close()
                .Close()
            .Close()
        .Close();

        using var context = Factory.CreateDbContext();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(false, 1);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_3"));
    }
    
    /// <summary>
    /// Test with multiple autobenched tests with a same priority, same number of worker threads, but different time managements.
    /// We expect, that test with the biggest time management will be returned.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_Running_DifferentTimeManagements_Autobenched()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch");
        
        EngineBuilder.AddAutobenchedTestForUser("test_1", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1,  state: TestState.Autobenched, workerThreads: [1], timeManagement: "8+0.08");

        EngineBuilder.AddAutobenchedTestForUser("test_2", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1, state: TestState.Autobenched, workerThreads: [1], timeManagement: "60+0.6");
        
        EngineBuilder.AddAutobenchedTestForUser("test_3", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1, state: TestState.Autobenched, workerThreads: [1], timeManagement: "120+1.2");
        
        
        using var context = Factory.CreateDbContext();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(true, 1);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_3"));
    }
    
    /// <summary>
    /// Test with multiple running tests with a same priority, same number of worker threads same TM, but different required worker threads.
    /// We expect, that test with the biggest number of worker threads will be returned.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_Running_SameTimeManagement_DifferentNumberThreads()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Running, numberOfThreads: 4)
                        .AddWorker(1)       
                    .Close()        
                    .AddTest("test_2", "test_book", "base_branch", "test_branch", state: TestState.Running, numberOfThreads: 2)
                        .AddWorker(1)
                    .Close()        
                    .AddTest("test_3", "test_book", "base_branch", "test_branch", state: TestState.Running, numberOfThreads: 1)
                        .AddWorker(1)
                    .Close()
                .Close()
            .Close()
        .Close();

        using var context = Factory.CreateDbContext();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(false, 16);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_1"));
    }
    
    
    /// <summary>
    /// Test with multiple autobenched with a same priority, same number of worker threads same TM, but different required worker threads.
    /// We expect, that test with the biggest number of worker threads will be returned.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_Running_SameTimeManagement_DifferentNumberThreads_Autobench()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch");

        
        EngineBuilder.AddAutobenchedTestForUser("test_1", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1,  state: TestState.Autobenched, workerThreads: [1], numberOfThreads: 4);

        EngineBuilder.AddAutobenchedTestForUser("test_2", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1, state: TestState.Autobenched, workerThreads: [1], numberOfThreads: 2);
        
        EngineBuilder.AddAutobenchedTestForUser("test_3", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1, state: TestState.Autobenched, workerThreads: [1], numberOfThreads: 1);

        
        using var context = Factory.CreateDbContext();
        
        var testStore = new TestStore(Factory);
        var nextTest = testStore.GetNextTestForWorker(true, 16);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_1"));
    }
    #endregion


    /// <summary>
    /// Test if test will be stopped.
    /// </summary>
    [Test]
    public async Task StopTest()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Running, numberOfThreads: 4)
                        .AddWorker(1)       
                    .Close()
                .Close()
            .Close()
        .Close();
        
        var test = GetTestByName(Factory,"test_1");
        Assert.That(test.State,Is.EqualTo(TestState.Running));
        
        var testStore = new TestStore(Factory);
        await testStore.StopTest(test.Id);

        test = GetTestByName(Factory,"test_1");
        Assert.That(test.State, Is.EqualTo(TestState.Stopped));
    }


    /// <summary>
    /// Test for normal test: SetRunningState. 
    /// </summary>
    [TestCase(TestState.Paused, TestState.Running)]
    [TestCase(TestState.Stopped, TestState.Stopped)]
    [TestCase(TestState.Running, TestState.Running)]
    public void SetRunningState_Normal(TestState initialState, TestState expectedState)
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: initialState, numberOfThreads: 4)
                        .AddWorker(1)       
                    .Close()
                .Close()
            .Close()
        .Close();
        
        var store = new TestStore(Factory);
        var test = GetTestByName(Factory,"test_1");
        Assert.That(test.State, Is.EqualTo(initialState));
        store.SetRunningState(test);
        
        test = GetTestByName(Factory,"test_1");
        Assert.That(test.State, Is.EqualTo(expectedState));
    }
    
    /// <summary>
    /// Test for autobenched test: SetRunningState. 
    /// </summary>
    [TestCase(TestState.Paused, TestState.Autobenched, 0.5, 1)]
    [TestCase(TestState.Paused, TestState.Running, 0.5, 2)]
    [TestCase(TestState.Paused, TestState.Running, 0.5, 5)]
    [TestCase(TestState.Paused, TestState.Autobenched, 0.1, 5)]
    public void SetRunningState_Autobenched(TestState initialState, TestState expectedState, double userConfidence, int updateIters)
    { 
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddAutobenchedTest("test_1", "test_book", "base_branch", "test_branch",state: initialState, bench:1561651, userConfidence: userConfidence)
                    .Close()
                .Close()
            .Close()
        .Close();
        
        var test = GetTestByNameAutobenched(Factory,"test_1");
        Assert.That(test.State, Is.EqualTo(initialState));
        
        for (var i = 0; i < updateIters; i++)
        {
            var context = Factory.CreateDbContext();
            var autobenchState = context.AutobenchStates.First(abs => abs.TestId == test.Id);
            
            autobenchState.UpdateConfidence(1561651);
            context.AutobenchStates.Update(autobenchState);
            context.SaveChanges();
            
            var tmpTest = GetTestByNameAutobenched(Factory, "test_1");
            var tmpStore = new TestStore(Factory);
            tmpStore.SetRunningState(tmpTest);
        }
        
        test = GetTestByNameAutobenched(Factory,"test_1");
        Assert.That(test.State, Is.EqualTo(expectedState));
    }
    
    /// <summary>
    /// Tests <see cref="TestStore.SetState(int, TestState)"/>.
    /// </summary>
    [TestCase(TestState.Paused, TestState.Running)]
    [TestCase(TestState.Running, TestState.Autobenched)]
    [TestCase(TestState.Running, TestState.Finished)]
    public async Task SetState_Id(TestState initialState, TestState expectedState)
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: initialState, numberOfThreads: 4)
                        .AddWorker(1)       
                    .Close()
                .Close()
            .Close()
        .Close();
        
        var test = GetTestByName(Factory,"test_1");
        Assert.That(test.State, Is.EqualTo(initialState));
        var testStore = new TestStore(Factory);
        
        await testStore.SetState(test.Id, expectedState);
        test = GetTestByName(Factory,"test_1");
        Assert.That(test.State, Is.EqualTo(expectedState));
    }
    
    /// <summary>
    /// Tests <see cref="TestStore.SetState(Test, TestState)"/>.
    /// </summary>
    [TestCase(TestState.Paused, TestState.Running)]
    [TestCase(TestState.Running, TestState.Autobenched)]
    [TestCase(TestState.Running, TestState.Finished)]
    public void SetState_Entity(TestState initialState, TestState expectedState)
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: initialState, numberOfThreads: 4)
                        .AddWorker(1)       
                    .Close()
                .Close()
            .Close()
        .Close();
        
        var test = GetTestByName(Factory,"test_1");
        Assert.That(test.State, Is.EqualTo(initialState));
        
        var testStore = new TestStore(Factory);
        testStore.SetState(test, expectedState);
        
        test = GetTestByName(Factory,"test_1");
        Assert.That(test.State, Is.EqualTo(expectedState));
    }

    /// <summary>
    /// Test for <see cref="TestStore.GetById" />.
    /// </summary>
    [Test]
    public void GetById()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
            .AddEngine("stockfish")
                .AddBranch("base_branch")
                .AddBranch("test_branch")
                .AddTest("test_1", "test_book", "base_branch", "test_branch", numberOfThreads: 4)
                    .EnsurePentaCreated(Factory.CreateDbContext())
                    .AddWorker(1)
                    .Close()
                .Close()
            .Close()
        .Close();

        var testStore = new TestStore(Factory);
        var testId = Factory.CreateDbContext().Tests.First().Id;
        var test = testStore.GetById(testId);
        
        Assert.That(test, Is.Not.Null);
        Assert.That(test.Penta, Is.Not.Null);
        Assert.That(test.TestBranch, Is.Not.Null);
        Assert.That(test.BaseBranch, Is.Not.Null);
        Assert.That(test.Settings, Is.Not.Null);
    }

    /// <summary>
    /// Test for <see cref="TestStore.RecentTests" />
    /// </summary>
    [Test]
    public void RecentTests()
    {
        var engineBuilder = new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch");
        
        for (var i = 0; i < 5; i++)
        {
            engineBuilder
                .AddTest($"test_{i}", "test_book", "base_branch", "test_branch");
        }
        
        var testStore = new TestStore(Factory);
        var engine = Factory.CreateDbContext().Engines.First();
        var recent = testStore.RecentTests(engine.Id, 3);

        Assert.That(recent, Has.Count.EqualTo(3));
        Assert.That(recent[0].Name, Is.EqualTo("test_4"));
        Assert.That(recent[1].Name, Is.EqualTo("test_3"));
        Assert.That(recent[2].Name, Is.EqualTo("test_2"));
    }
    
    private static Test GetTestByName(TestContextFactory factory, string name)
    {
        using var context = factory.CreateDbContext();
       
        return context.Tests
            .AsNoTracking()
            .First(t => t.Name == name);
    }
    
    private static Test GetTestByNameAutobenched(TestContextFactory factory, string name)
    {
        using var context = factory.CreateDbContext();
       
        return context.Tests
            .AsNoTracking()
            .Include(t => t.AutobenchState)
            .First(t => t.Name == name);
    }
}
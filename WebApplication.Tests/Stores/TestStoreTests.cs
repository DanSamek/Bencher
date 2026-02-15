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
                    .AddTest("test_2", "test_book", "base_branch", "test_branch", state: TestState.Finished, ended: new DateTime(2000,1 ,1).ToUniversalTime(), priority: 5)
                        .Close()
                    .AddTest("test_3", "test_book", "base_branch", "test_branch", state: TestState.Stopped, ended: new DateTime(2000,1 ,1).ToUniversalTime(), priority: 5)
                        .Close()
                .Close()
            .Close();
        
        var testStore = CreateTestStore();
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
        
        var testStore = CreateTestStore();
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
            .CreateSprtSettings()
            .CreateUser("test_user_x")
            .AddEngine("stockfish_x")
            .AddBranch("base_branch_x")
            .AddBranch("test_branch_x")
            .AddTest("test_X2", "test_book", "base_branch_x", "test_branch_x", state: TestState.Finished,
                ended: new DateTime(2000, 1, 1).ToUniversalTime(), priority: 5)
            .Close()
            .AddTest("test_X3", "test_book", "base_branch_x", "test_branch_x", state: TestState.Stopped,
                ended: new DateTime(2000, 1, 1).ToUniversalTime(), priority: 5)
            .Close()
            .Close()
            .Close();
        
        var testStore = CreateTestStore();
     
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
        
        Assert.That(Factory.CreateDbContext().Tests.Count(), Is.EqualTo(7));
        
        var nextTests = new HashSet<string>();
        for (var i = 0; i < 5; i++)
        {
            var result = testStore.GetNextTestForWorker(false, 1);
            Assert.That(result, Is.Not.Null);
            nextTests.Add(result.Name);
            
            var tmpStore = CreateTestStore();
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
            .CreateUser("test_user_x")
            .AddEngine("stockfish_x")
            .AddBranch("base_branch_x")
            .AddBranch("test_branch_x")
            .Close()
            .Close()
            .Close();
        
        EngineBuilder.AddAutobenchedTestForUser("test_X2", "test_book", "base_branch_x", "test_branch_x", "stockfish_x",
            "test_user_x", Factory.CreateDbContext(), numberOfThreads: 2, state: TestState.Stopped, priority: 5, ended: new DateTime(2000,1,1).ToUniversalTime());
        
        EngineBuilder.AddAutobenchedTestForUser("test_X2", "test_book", "base_branch_x", "test_branch_x", "stockfish_x",
            "test_user_x", Factory.CreateDbContext(), numberOfThreads: 2, state: TestState.Finished, priority: 5, ended: new DateTime(2000,1,1).ToUniversalTime());

        var testStore = CreateTestStore();

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

        Assert.That(Factory.CreateDbContext().Tests.Count(), Is.EqualTo(7));
        Assert.That(Factory.CreateDbContext().AutobenchStates.Count(), Is.EqualTo(7));

        var nextTests = new HashSet<string>();
        for (var i = 0; i < 5; i++)
        {
            var result = testStore.GetNextTestForWorker(true, 1);
            Assert.That(result, Is.Not.Null);
            nextTests.Add(result.Name);

            var tmpStore = CreateTestStore();
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
        
        var testStore = CreateTestStore();
        var nextTest = testStore.GetNextTestForWorker(false, 1);
        Assert.That(nextTest, Is.Null);
    }
    
    /// <summary>
    /// Test with multiple stopped tests, but all of them requires more cores, than worker's cores.
    /// Autobench variant - HERE is a catch, that autobench requires only ONE thread.
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
        
        Assert.That(Factory.CreateDbContext().AutobenchStates.Count(),Is.EqualTo(3));
        Assert.That(Factory.CreateDbContext().Tests.Count(t => t.Autobenched), Is.EqualTo(3));
        Assert.That(Factory.CreateDbContext().Tests.Include(t => t.AutobenchState).Count(t => t.AutobenchState != null), Is.EqualTo(3));

        var testStore = CreateTestStore();
        var nextTest = testStore.GetNextTestForWorker(true, 1);
        Assert.That(nextTest, Is.Not.Null);
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
        
        var testStore = CreateTestStore();
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
        
        Assert.That(factory.CreateDbContext().AutobenchStates.Count(), Is.EqualTo(3));
        Assert.That(factory.CreateDbContext().Tests.Count(t => t.Autobenched), Is.EqualTo(3));
        Assert.That(factory.CreateDbContext().Tests.Include(t => t.AutobenchState).Count(t => t.AutobenchState != null), Is.EqualTo(3));

        var testStore = CreateTestStore();
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
        
        var testStore = CreateTestStore();
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
        
        var testStore = CreateTestStore();
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
        
        var testStore = CreateTestStore();
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
        
        var testStore = CreateTestStore();
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
        
        var testStore = CreateTestStore();
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
        
        var testStore = CreateTestStore();
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
        
        var testStore = CreateTestStore();
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
        
        var testStore = CreateTestStore();
        var nextTest = testStore.GetNextTestForWorker(true, 16);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_1"));
    }
    
    /// <summary>
    /// Test validates, that if there are not any paused/running/autobenched tests, null will be returned.
    /// Normal test version.
    /// </summary>
    [TestCase(TestState.Finished)]
    [TestCase(TestState.Stopped)]
    public void GetNextTestForWorker_TestsStoppedOrFinished(TestState testState)
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch");
        
        EngineBuilder.AddTestForUser("test_1", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1,  state: testState, ended: new DateTime(2000,1,1).ToUniversalTime());
        
        using var context = Factory.CreateDbContext();
        
        var testStore = CreateTestStore();
        var nextTest = testStore.GetNextTestForWorker(false, 16);
        
        Assert.That(nextTest, Is.Null);
    }

    
    /// <summary>
    /// Test validates, that if there are not any paused/running/autobenched tests, null will be returned.
    /// Autobenched test version.
    /// </summary>
    [TestCase(TestState.Finished)]
    [TestCase(TestState.Stopped)]
    public void GetNextTestForWorker_TestsStoppedOrFinished_Autobenched(TestState testState)
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch");
        
        EngineBuilder.AddAutobenchedTestForUser("test_1", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1,  state: testState, ended: new DateTime(2000,1,1).ToUniversalTime());
        
        using var context = Factory.CreateDbContext();
        
        var testStore = CreateTestStore();
        var nextTest = testStore.GetNextTestForWorker(true, 16);
        
        Assert.That(nextTest, Is.Null);
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
        
        var testStore = CreateTestStore();
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
        DateTime? date = initialState is TestState.Stopped or TestState.Finished ? DateTime.UtcNow : null;
        
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: initialState, numberOfThreads: 4, ended:date)
                        .AddWorker(1)       
                    .Close()
                .Close()
            .Close()
        .Close();
        
        var store = CreateTestStore();
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
            var tmpStore = CreateTestStore();
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
        var testStore = CreateTestStore();
        
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
        
        var testStore = CreateTestStore();
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

        var testStore = CreateTestStore();
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
        
        var testStore = CreateTestStore();
        var engine = Factory.CreateDbContext().Engines.First();
        var recent = testStore.RecentTests(engine.Id, 3);

        Assert.That(recent, Has.Count.EqualTo(3));
        Assert.That(recent[0].Name, Is.EqualTo("test_4"));
        Assert.That(recent[1].Name, Is.EqualTo("test_3"));
        Assert.That(recent[2].Name, Is.EqualTo("test_2"));
    }

    /// <summary>
    /// Tests <see cref="TestStore.GetRunningTests" />.
    /// </summary>
    [Test]
    public void GetRunningTests()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Running)
                        .EnsurePentaCreated(Factory.CreateDbContext())
                        .AddWorker(1)
                        .Close()
                    .AddTest("test_2", "test_book", "base_branch", "test_branch")
                        .Close()
                    .AddTest("test_3", "test_book", "base_branch", "test_branch")
                        .Close()
                .Close()
            .Close()
        .Close();
        
        EngineBuilder.AddAutobenchedTestForUser("test_4", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1, state: TestState.Autobenched, workerThreads: [1]);
        
        var store = CreateTestStore();

        var (autobenchedTests, runningTests) = store.GetRunningTests();
        var tests = new List<Test>(autobenchedTests);
        tests.AddRange(runningTests);
        
        tests = tests.OrderBy(t => t.Name).ToList();
        Assert.That(tests, Has.Count.EqualTo(2));
        Assert.That(tests[0].Name, Is.EqualTo("test_1"));
        Assert.That(tests[1].Name, Is.EqualTo("test_4"));
    }
    
    /// <summary>
    /// Tests <see cref="TestStore.GetPausedTests"/>
    /// </summary>
    [Test]
    public void GetPausedTests()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Finished, ended: DateTime.UtcNow)
                        .EnsurePentaCreated(Factory.CreateDbContext())
                        .AddWorker(1)
                        .Close()
                    .AddTest("test_2", "test_book", "base_branch", "test_branch")
                        .Close()
                    .AddTest("test_3", "test_book", "base_branch", "test_branch")
                        .Close()
                    .Close()
              .Close()
        .Close();
        
        var store = CreateTestStore();
        var pausedTests= store.GetPausedTests();
        pausedTests = pausedTests.OrderBy(t => t.Name).ToList();
        Assert.That(pausedTests, Has.Count.EqualTo(2));
        Assert.That(pausedTests[0].Name, Is.EqualTo("test_2"));
        Assert.That(pausedTests[1].Name, Is.EqualTo("test_3"));
    }
    
    
    /// <summary>
    /// Tests <see cref="TestStore.SetFinishedState"/>
    /// </summary>
    [Test]
    public async Task SetFinishedState()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Running)
                        .EnsurePentaCreated(Factory.CreateDbContext())
                        .AddWorker(1)
                        .Close()
                    .Close()
                .Close()
            .Close();
        
        var store = CreateTestStore();
        var test = GetTestByName(Factory, "test_1"); 
        Assert.That(test.State, Is.EqualTo(TestState.Running));
        
        await store.SetFinishedState(test.Id);
        test = GetTestByName(Factory, "test_1"); 
        Assert.That(test.State, Is.EqualTo(TestState.Finished));
    }

    /// <summary>
    /// Tests <see cref="TestStore.GetPassedTestsForPage" />. 
    /// </summary>
    [Test]
    public async Task GetPassedTestsForPage()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch")
            .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Finished, ended: new DateTime(2000,1,1).ToUniversalTime())
            .EnsurePentaCreated(Factory.CreateDbContext())
            .Close()
            .AddTest("test_2", "test_book", "base_branch", "test_branch", state: TestState.Finished, ended: new DateTime(2005,1,1).ToUniversalTime())
            .EnsurePentaCreated(Factory.CreateDbContext())
            .Close()
            .AddTest("test_3", "test_book", "base_branch", "test_branch")
            .EnsurePentaCreated(Factory.CreateDbContext())
            .Close()
            .AddTest("test_4", "test_book", "base_branch", "test_branch", state: TestState.Finished, ended: new DateTime(1995,1,1).ToUniversalTime())
            .Close()
            .AddTest("test_5", "test_book", "base_branch", "test_branch", state: TestState.Finished, ended: new DateTime(2010,1,1).ToUniversalTime())
            .Close()
            .Close()
            .Close()
            .Close();
        
        var store = CreateTestStore();
        var pentaStore = new PentaStore(Factory);
        AddPentaToTest("test_1");
        await pentaStore.UpdatePenta(GetTestByName(Factory, "test_1").Id, 1000,0,0,0,0,0);
        
        AddPentaToTest("test_2");
        await pentaStore.UpdatePenta(GetTestByName(Factory, "test_2").Id, 100,100,100,100,100,2000);
        
        AddPentaToTest("test_4");
        await pentaStore.UpdatePenta(GetTestByName(Factory, "test_4").Id, 100,100,100,100,100,2000);
        
        AddPentaToTest("test_5");
        await pentaStore.UpdatePenta(GetTestByName(Factory, "test_5").Id, 100,100,100,100,100,2000);
        
        var passedTests = store.GetPassedTestsForPage(0, 2);
        Assert.That(passedTests.Count, Is.EqualTo(2));
        Assert.That(passedTests[0].Name, Is.EqualTo("test_5"));
        Assert.That(passedTests[1].Name, Is.EqualTo("test_2"));
        
        passedTests = store.GetPassedTestsForPage(1, 2);
        Assert.That(passedTests.Count, Is.EqualTo(1));
        Assert.That(passedTests[0].Name, Is.EqualTo("test_4"));
        
        passedTests = store.GetPassedTestsForPage(2, 2);
        Assert.That(passedTests, Is.Empty);
        
        return;

        void AddPentaToTest(string testName)
        {
            var context = Factory.CreateDbContext();
            var test = context.Tests.First(t => t.Name == testName);
            var penta = new Penta
            {
                Test = test,
                TestId = test.Id
            };
            
            penta = context.Pentas.Add(penta).Entity;
            context.SaveChanges();
            test.Penta = penta;
            context.SaveChanges();
        }
    }


    /// <summary>
    /// Tests <see cref="TestStore.SetPausedIfNoActiveWorkers"/>
    /// Test is going to have active workers -> test should be still in the running state.
    /// </summary>
    [Test]
    public async Task SetPausedIfNoActiveWorkers_ShouldBeRunning()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Running)
                        .EnsurePentaCreated(Factory.CreateDbContext())
                        .AddWorker(1)
                        .Close()
                    .Close()
                .Close()
            .Close();

        var test = GetTestByName(Factory, "test_1");
        Assert.That(test.State, Is.EqualTo(TestState.Running));
        var store = CreateTestStore();
        var running = await store.SetPausedIfNoActiveWorkers(test.Id);
        
        test = GetTestByName(Factory, "test_1");
        
        Assert.That(running);
        Assert.That(test.State, Is.EqualTo(TestState.Running));
    }
    
    /// <summary>
    /// Tests <see cref="TestStore.SetPausedIfNoActiveWorkers"/>
    /// Test is going to have no active workers -> test should be in the paused state.
    /// </summary>
    [Test]
    public async Task SetPausedIfNoActiveWorkers_ShouldBePaused()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Running)
                        .EnsurePentaCreated(Factory.CreateDbContext())
                        .AddWorker(1)
                        .AddWorker(1)
                        .Close()
                    .Close()
                .Close()
            .Close();

        var test = GetTestByName(Factory, "test_1");
        Assert.That(test.State, Is.EqualTo(TestState.Running));
        var store = CreateTestStore();
        var running = await store.SetPausedIfNoActiveWorkers(test.Id);
        Assert.That(running);
        
        test = GetTestByName(Factory, "test_1");
        Assert.That(test.State, Is.EqualTo(TestState.Running));

        await Factory.CreateDbContext()
            .WorkerLogs
            .ExecuteUpdateAsync(spc => spc.SetProperty(wl => wl.State, WorkerLogState.Finished));
        
        running = await store.SetPausedIfNoActiveWorkers(test.Id);
        test = GetTestByName(Factory, "test_1");

        Assert.That(!running);
        Assert.That(test.State, Is.EqualTo(TestState.Paused));
    }
    
    /// <summary>
    /// Tests <see cref="TestStore.GetEndedTestsForPage"/> with the page size 2.
    /// </summary>
    [Test]
    public void GetEndedTestsForPage()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch")
            .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Finished, ended: new DateTime(2000,1,1).ToUniversalTime())
            .Close()
            .AddTest("test_2", "test_book", "base_branch", "test_branch", state: TestState.Stopped,  ended: new DateTime(2100,1,1).ToUniversalTime())
            .Close()
            .AddTest("test_3", "test_book", "base_branch", "test_branch", state: TestState.Finished,  ended: new DateTime(2030,1,1).ToUniversalTime())
            .Close()
            .AddTest("test_4", "test_book", "base_branch", "test_branch", state: TestState.Stopped,  ended: new DateTime(2010,1,1).ToUniversalTime())
            .Close()
            .AddTest("test_5", "test_book", "base_branch", "test_branch", state: TestState.Finished,  ended: new DateTime(1989,1,1).ToUniversalTime())
            .Close()
            .AddTest("test_6", "test_book", "base_branch", "test_branch", state: TestState.Finished,  ended: new DateTime(2080,1,1).ToUniversalTime())
            .Close()
            .AddTest("test_7", "test_book", "base_branch", "test_branch", state: TestState.Stopped,  ended: new DateTime(2039,1,1).ToUniversalTime())
            .Close()
            .Close()
            .Close();

        var testCount = Factory.CreateDbContext().Tests.Count();
        Assert.That(testCount, Is.EqualTo(7));
        
        var expectedPages = new List<string[]>();
        expectedPages.Add(["test_2", "test_6"]);
        expectedPages.Add(["test_7", "test_3"]);
        expectedPages.Add(["test_4", "test_1"]);
        expectedPages.Add(["test_5"]);
        expectedPages.Add([]);
        var store = CreateTestStore();

        for (var pageQuery = 0; pageQuery <= 4; pageQuery++)
        {
            var result = store
                .GetEndedTestsForPage(pageQuery, pageSize: 2)
                .Select(t => t.Name)
                .ToArray();

            Assert.That(result, Is.EqualTo(expectedPages[pageQuery]));
        }
    }
    
    /// <summary>
    /// Tests <see cref="TestStore.TotalPausedTestsWithMaxPriority"/>.
    /// </summary>
    [Test]
    public void TotalPausedTestsWithMaxPriority()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch")
            .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Paused, priority: 1)
            .Close()
            .AddTest("test_3", "test_book", "base_branch", "test_branch", state: TestState.Paused, priority: 0)
            .Close()
            .AddTest("test_4", "test_book", "base_branch", "test_branch", state: TestState.Paused, priority: -1)
            .Close()
            .AddTest("test_50", "test_book", "base_branch", "test_branch", state: TestState.Stopped, priority: 2, numberOfThreads: 64, ended: new DateTime(2000,1,1).ToUniversalTime())
            .Close()
            .AddTest("test_50", "test_book", "base_branch", "test_branch", state: TestState.Finished, priority: 2, numberOfThreads: 64, ended: new DateTime(2000,1,1).ToUniversalTime())
            .Close()
            .AddTest("test_2", "test_book", "base_branch", "test_branch", state: TestState.Running)
            .EnsurePentaCreated(Factory.CreateDbContext())
            .AddWorker(1)
            .Close()
            .Close()
            .Close()
            .Close();
        
        EngineBuilder.AddAutobenchedTestForUser("test_5", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 1, state: TestState.Autobenched, workerThreads: [1]);
        
        EngineBuilder.AddAutobenchedTestForUser("test_6", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 2, state: TestState.Paused);
        
        EngineBuilder.AddAutobenchedTestForUser("test_7", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 2, state: TestState.Paused);
        
        var store = CreateTestStore();
        var result = store.TotalPausedTestsWithMaxPriority();
        Assert.That(result,Is.EqualTo(2));
    }
    
    
    /// <summary>
    /// Tests <see cref="TestStore.MaxThreadsForTestWithMaxPriority"/>.
    /// </summary>
    [Test]
    public void MaxThreadsForTestWithMaxPriority()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch")
            .AddTest("test_1", "test_book", "base_branch", "test_branch", state: TestState.Paused, priority: 1)
            .Close()
            .AddTest("test_3", "test_book", "base_branch", "test_branch", state: TestState.Paused, priority: 0)
            .Close()
            .AddTest("test_4", "test_book", "base_branch", "test_branch", state: TestState.Paused, priority: -1)
            .Close()
            .AddTest("test_50", "test_book", "base_branch", "test_branch", state: TestState.Stopped, priority: 2, numberOfThreads: 64, ended: new DateTime(2000,1,1).ToUniversalTime())
            .Close()
            .AddTest("test_50", "test_book", "base_branch", "test_branch", state: TestState.Finished, priority: 2, numberOfThreads: 64, ended: new DateTime(2000,1,1).ToUniversalTime())
            .Close()
            .AddTest("test_2", "test_book", "base_branch", "test_branch", state: TestState.Running, priority: 2, numberOfThreads:8)
            .EnsurePentaCreated(Factory.CreateDbContext())
            .AddWorker(1)
            .Close()
            .Close()
            .Close()
            .Close();
        
        EngineBuilder.AddAutobenchedTestForUser("test_5", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 2, state: TestState.Autobenched, workerThreads: [1], numberOfThreads: 4);
        
        EngineBuilder.AddAutobenchedTestForUser("test_6", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 2, state: TestState.Paused, numberOfThreads: 16);
        
        EngineBuilder.AddAutobenchedTestForUser("test_7", "test_book", "base_branch", "test_branch", "stockfish",
            "test_user", Factory.CreateDbContext(), priority: 2, state: TestState.Paused);
        
        var store = CreateTestStore();
        var result = store.MaxThreadsForTestWithMaxPriority();
        Assert.That(result,Is.EqualTo(16));
    }

    /// <summary>
    /// Tests <see cref="TestStore.UpdatePriority" />.
    /// We expect that after method call, priority is changed. 
    /// </summary>
    [Test]
    public void UpdatePriority()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch")
            .AddTest("test_1", "test_book", "base_branch", "test_branch", priority: 1)
            .Close()
            .Close()
            .Close()
            .Close();

        var store = CreateTestStore();
        var test = GetTestByName(Factory, "test_1");
        Assert.That(test.Priority, Is.EqualTo(1));
        
        store.UpdatePriority(test.Id, 5);
        
        test = GetTestByName(Factory, "test_1");
        Assert.That(test.Priority, Is.EqualTo(5));
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
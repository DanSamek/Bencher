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
    [Test]
    public void GetNextTestForWorker_NoRunningTest()
    {
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                    .AddTest("test_1", "test_book", "base_branch", "test_branch")
                .Close()
            .Close();
        
        var testStore = new TestStore(factory);
        var nextTest = testStore.GetNextTestForWorker(false, 1);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_1"));
    }
    
    /// <summary>
    /// Test with the multiple paused tests,
    /// we expect, that tests one by one will be picked by a store mechanism.
    ///     - We manually set state to a running, because this logic handles <see cref="WebApplication.API.WorkerController.RunningTest"/>. [TODO change reference, this will handle store]
    /// </summary>
    [Test]
    public void GetNextTestForWorker_AllTestsWithWorkers()
    {
        var factory = CreateContextFactory();
        var domainBuilder = new DomainBuilder(factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings();
        
        var testStore = new TestStore(factory);
        
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
        
        Assert.That(5, Is.EqualTo(factory.CreateDbContext().Tests.Count()));
        
        var nextTests = new HashSet<string>();
        for (var i = 0; i < 5; i++)
        {
            var result = testStore.GetNextTestForWorker(false, 1);
            Assert.That(result, Is.Not.Null);
            nextTests.Add(result.Name);
            
            var tmpStore = new TestStore(factory);
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
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
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
        
        var testStore = new TestStore(factory);
        var nextTest = testStore.GetNextTestForWorker(false, 1);
        Assert.That(nextTest, Is.Null);
    }
    
    /// <summary>
    /// Test with multiple stopped tests, but all of them has different priorities.
    /// We expect, that test with the highest priority will be selected.
    /// </summary>
    [Test]
    public void GetNextTestForWorker_BiggestPriority()
    {
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
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
        
        var testStore = new TestStore(factory);
        var nextTest = testStore.GetNextTestForWorker(false, 1);
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
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
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

        using var context = factory.CreateDbContext();
        
        var testStore = new TestStore(factory);
        var nextTest = testStore.GetNextTestForWorker(false, 1);
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
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
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

        using var context = factory.CreateDbContext();
        
        var testStore = new TestStore(factory);
        var nextTest = testStore.GetNextTestForWorker(false, 1);
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
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
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

        using var context = factory.CreateDbContext();
        
        var testStore = new TestStore(factory);
        var nextTest = testStore.GetNextTestForWorker(false, 1);
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
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
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

        using var context = factory.CreateDbContext();
        
        var testStore = new TestStore(factory);
        var nextTest = testStore.GetNextTestForWorker(false, 16);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_1"));
    }
    
    // TODO autobench variants!
    #endregion


    /// <summary>
    /// Test if test will be stopped.
    /// </summary>
    [Test]
    public async Task StopTest()
    {
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
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
        
        var test = GetTestByName(factory,"test_1");
        Assert.That(test.State,Is.EqualTo(TestState.Running));
        
        var testStore = new TestStore(factory);
        await testStore.StopTest(test.Id);

        test = GetTestByName(factory,"test_1");
        Assert.That(test.State, Is.EqualTo(TestState.Stopped));
    }


    /// <summary>
    /// Test for paused non-autobenched test - SetRunningState. 
    /// </summary>
    [TestCase(TestState.Paused, TestState.Running)]
    [TestCase(TestState.Stopped, TestState.Stopped)]
    [TestCase(TestState.Running, TestState.Running)]
    public void SetRunningState_Normal(TestState initialState, TestState expectedState)
    {
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
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
        
        var store = new TestStore(factory);
        var test = GetTestByName(factory,"test_1");
        Assert.That(test.State, Is.EqualTo(initialState));
        store.SetRunningState(test);
        
        test = GetTestByName(factory,"test_1");
        Assert.That(test.State, Is.EqualTo(expectedState));
    }
    
    
    // TODO
    public void SetRunningState_Autobenched(TestState initialState, TestState expectedState)
    {
       
    }
    
    /// <summary>
    /// Tests <see cref="TestStore.SetState(int, TestState)"/>.
    /// </summary>
    [TestCase(TestState.Paused, TestState.Running)]
    [TestCase(TestState.Running, TestState.Autobenched)]
    [TestCase(TestState.Running, TestState.Finished)]
    public async Task SetState_Id(TestState initialState, TestState expectedState)
    {
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
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
        
        var test = GetTestByName(factory,"test_1");
        Assert.That(test.State, Is.EqualTo(initialState));
        var testStore = new TestStore(factory);
        
        await testStore.SetState(test.Id, expectedState);
        test = GetTestByName(factory,"test_1");
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
        var factory = CreateContextFactory();
        new DomainBuilder(factory.CreateDbContext())
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
        
        var test = GetTestByName(factory,"test_1");
        Assert.That(test.State, Is.EqualTo(initialState));
        
        var testStore = new TestStore(factory);
        testStore.SetState(test, expectedState);
        
        test = GetTestByName(factory,"test_1");
        Assert.That(test.State, Is.EqualTo(expectedState));
    }

    private static Test GetTestByName(TestContextFactory factory, string name)
    {
        using var context = factory.CreateDbContext();
       
        return context.Tests
            .AsNoTracking()
            .First(t => t.Name == name);
    } 
}
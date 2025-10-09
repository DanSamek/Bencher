using WebApplication.Data.Models;
using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class TestStoreTests : TestBase
{
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
            
            // We handle running state when worker-api/running is called. [TODO store]
            result.State = result.AutobenchState is null || result.AutobenchState!.Resolved ? TestState.Running : TestState.Autobenched;
            using var tmpContext = factory.CreateDbContext();
            tmpContext.Tests.Update(result);
            tmpContext.SaveChanges();
        }
        
        Assert.That(nextTests, Has.Count.EqualTo(5));
    } 
}
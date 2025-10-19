using Microsoft.EntityFrameworkCore;
using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class TestBranchStoreTests : TestBase
{
    /// <summary>
    /// Test for <see cref="TestBranchStore.SetTestBranchBench"/>.
    /// </summary>
    [Test]
    public async Task SetTestBranchBench()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("test_branch")
                    .AddBranch("base_branch")
                .Close()
            .Close()
        .Close();
        
        EngineBuilder.AddAutobenchedTestForUser("test_1", "test_book", "base_branch", 
            "test_branch", "stockfish", "test_user",Factory.CreateDbContext(), bench: 123456789);
        
        var testId = Factory.CreateDbContext().Tests.First().Id;
        var store = new TestBranchStore(Factory);
        await store.SetTestBranchBench(testId, 987654321);
        
        var test = Factory.CreateDbContext().Tests
            .Include(t => t.TestBranch)
            .First();
        
        Assert.That(test.TestBranch.Bench, Is.EqualTo(987654321));
    }
}
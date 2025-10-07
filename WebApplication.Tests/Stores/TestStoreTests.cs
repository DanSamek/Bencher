using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class TestStoreTests : TestBase
{
    [Test]
    // TODO [TestCases] ?
    public void GetNextTestForWorker_NoRunningTest()
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
        var nextTest = testStore.GetNextTestForWorker(false, 1);
        Assert.That(nextTest, Is.Not.Null);
        Assert.That(nextTest.Name, Is.EqualTo("test_1"));
    }
}
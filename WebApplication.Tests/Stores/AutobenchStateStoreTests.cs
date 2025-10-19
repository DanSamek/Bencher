using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class AutobenchStateStoreTests : TestBase
{
    /// <summary>
    /// Test for  <see cref="AutobenchStateStore.GetAutobenchStateByTestId"/>.
    /// </summary>
    [Test]
    public void GetAutobenchStateByTestId()
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
            "test_branch", "stockfish", "test_user", Factory.CreateDbContext());
        
        EngineBuilder.AddAutobenchedTestForUser("test_2", "test_book", "base_branch", 
            "test_branch", "stockfish", "test_user", Factory.CreateDbContext());
        
        var store = new AutobenchStateStore(Factory);
        var test = Factory.CreateDbContext().Tests.First(t => t.Name == "test_1");
        var autobenchState = store.GetAutobenchStateByTestId(test.Id);
        
        Assert.That(autobenchState, Is.Not.Null);
        Assert.That(autobenchState.TestId, Is.EqualTo(test.Id));
    }
}
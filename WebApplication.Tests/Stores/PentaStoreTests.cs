using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class PentaStoreTests : TestBase
{
    /// <summary>
    /// Tests for <see cref="PentaStore.UpdatePenta" />. 
    /// </summary>
    [Test]
    public async Task UpdatePenta()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("test_branch")
            .AddBranch("base_branch")
            .AddTest("test_1", "test_book", "base_branch", "test_branch")
            .EnsurePentaCreated(Factory.CreateDbContext())
            .Close()
            .Close()
            .Close();
        
        var testId = Factory.CreateDbContext().Tests.First().Id;
        var store = new PentaStore(Factory);
        await store.UpdatePenta(testId,1,2,3,4,5,6);
        
        var penta = Factory.CreateDbContext().Pentas.First();
        Assert.That(penta.Id, Is.EqualTo(testId));
        Assert.That(penta.Ll, Is.EqualTo(1));
        Assert.That(penta.Ld, Is.EqualTo(2));
        Assert.That(penta.Dd, Is.EqualTo(3));
        Assert.That(penta.Wl, Is.EqualTo(4));
        Assert.That(penta.Wd, Is.EqualTo(5));
        Assert.That(penta.Ww, Is.EqualTo(6));
    }
}
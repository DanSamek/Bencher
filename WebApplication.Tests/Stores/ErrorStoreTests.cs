using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class ErrorStoreTests : TestBase
{
    /// <summary>
    /// Test for <see cref="ErrorStore.GetErrors" />, but no error is in the database.
    /// </summary>
    [Test]
    public void GetErrors_Empty()
    {
        var store = new ErrorStore(Factory);
        var errors = store.GetErrors();
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors, Is.Empty);
    }
    
    /// <summary>
    /// Test for <see cref="ErrorStore.GetErrors" />, with errors in the database.
    /// We expect, that errors will be returned by the newest ones.
    /// </summary>
    [Test]
    public void GetErrors()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
                .CreateUser("test_user")
                    .AddEngine("stockfish")
                        .AddBranch("test_branch")
                        .AddBranch("base_branch")
                        .AddTest("test_1", "test_book", "base_branch", "test_branch")
                            .AddWorker(1)
                            .AddError(Factory.CreateDbContext(), new DateTime(2015,4,4))
                            .AddError(Factory.CreateDbContext(), new DateTime(2000,5,4))
                            .AddError(Factory.CreateDbContext(), new DateTime(2005,5,4))
                            .EnsurePentaCreated(Factory.CreateDbContext())
                            .Close()
                        .Close()
                .Close()
            .Close();
        
        var store = new ErrorStore(Factory);
        var errors = store.GetErrors();
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors, Has.Count.EqualTo(3));
        Assert.That(errors[0].Time.Year, Is.EqualTo(2015));
        Assert.That(errors[1].Time.Year, Is.EqualTo(2005));
        Assert.That(errors[2].Time.Year, Is.EqualTo(2000));
    }
}
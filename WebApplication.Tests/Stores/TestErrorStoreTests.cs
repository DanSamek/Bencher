using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class TestErrorStoreTests : TestBase
{
    /// <summary>
    /// Test for <see cref="TestErrorStore.GetErrors" />, but no error is in the database.
    /// </summary>
    [Test]
    public void GetErrors_Empty()
    {
        var store = new TestErrorStore(Factory);
        var errors = store.GetErrors();
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors, Is.Empty);
    }
    
    /// <summary>
    /// Test for <see cref="TestErrorStore.GetErrors" />, with errors in the database.
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
        
        var store = new TestErrorStore(Factory);
        var errors = store.GetErrors();
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors, Has.Count.EqualTo(3));
        Assert.That(errors[0].Time.Year, Is.EqualTo(2015));
        Assert.That(errors[1].Time.Year, Is.EqualTo(2005));
        Assert.That(errors[2].Time.Year, Is.EqualTo(2000));
    }
    
   
    /// <summary>
    /// Tests <see cref="TestErrorStore.LoadContent" />.
    /// </summary>
    [Test]
    public void LoadContent()
    {
        var store = new TestErrorStore(Factory);
        var data = new byte[] { 0x11, 0x22, 0x33 };

        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("test_branch")
            .AddBranch("base_branch")
            .AddTest("test_1", "test_book", "base_branch", "test_branch")
            .AddWorker(1)
            .AddError(Factory.CreateDbContext(), new DateTime(2015,4,4), data: data)
            .EnsurePentaCreated(Factory.CreateDbContext())
            .Close()
            .Close()
            .Close()
            .Close();
        
        var id = Factory.CreateDbContext().TestErrors.First().Id;
        var content = store.LoadContent(id);
        
        Assert.That(content, Is.EqualTo(data));
    }

    /// <summary>
    /// Tests <see cref="TestErrorStore.GetErrorsForTest" />.
    /// </summary>
    [Test]
    public void GetErrorsForTest()
    {
        var store = new TestErrorStore(Factory);
        var data = new byte[] { 0x11, 0x22, 0x33 };

        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings()
            .CreateUser("test_user")
            .AddEngine("stockfish")
            .AddBranch("test_branch")
            .AddBranch("base_branch")
            .AddTest("test_1", "test_book", "base_branch", "test_branch")
            .AddWorker(1)
            .AddError(Factory.CreateDbContext(), new DateTime(2016,4,4), data: data)
            .AddError(Factory.CreateDbContext(), new DateTime(2018,1,4), data: data)
            .EnsurePentaCreated(Factory.CreateDbContext())
            .Close()
            .AddTest("test_2", "test_book", "base_branch", "test_branch")
            .AddWorker(1)
            .AddError(Factory.CreateDbContext(), new DateTime(2007,2,2), data: data)
            .AddError(Factory.CreateDbContext(), new DateTime(2021,4,4), data: data)
            .AddError(Factory.CreateDbContext(), new DateTime(2020,3,3), data: data)
            .EnsurePentaCreated(Factory.CreateDbContext())
            .Close()
            .Close()
            .Close()
            .Close();

        var testId = Factory.CreateDbContext().Tests.First(t => t.Name == "test_2").Id;
        var errors = store.GetErrorsForTest(testId);
        
        Assert.That(errors, Has.Count.EqualTo(3));
        Assert.That(errors[0].Time, Is.EqualTo(new DateTime(2021,4,4).ToUniversalTime()));
        Assert.That(errors[1].Time, Is.EqualTo(new DateTime(2020,3,3).ToUniversalTime()));
        Assert.That(errors[2].Time, Is.EqualTo(new DateTime(2007,2,2).ToUniversalTime()));
    }
}
using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class TestErrorStoreTests : TestBase
{
    /// <summary>
    /// Tests <see cref="TestErrorStore.GetErrorsForPage"/> with page size = 2.
    /// </summary>
    [Test]
    public void GetErrorsForPage()
    {
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
            .AddError(Factory.CreateDbContext(), new DateTime(2000,4,4).ToUniversalTime(), data: data)
            .AddError(Factory.CreateDbContext(), new DateTime(2001,4,4).ToUniversalTime(), data: data)
            .AddError(Factory.CreateDbContext(), new DateTime(2002,4,4).ToUniversalTime(), data: data)
            .AddError(Factory.CreateDbContext(), new DateTime(2003,4,4).ToUniversalTime(), data: data)
            .AddError(Factory.CreateDbContext(), new DateTime(1950,4,4).ToUniversalTime(), data: data)
            .AddError(Factory.CreateDbContext(), new DateTime(1948,4,4).ToUniversalTime(), data: data)
            .AddError(Factory.CreateDbContext(), new DateTime(2100,4,4).ToUniversalTime(), data: data)
            .EnsurePentaCreated(Factory.CreateDbContext())
            .Close()
            .Close()
            .Close()
            .Close();

        var expectedErrors = new List<DateTime[]>();
        expectedErrors.Add([new DateTime(2100,4,4).ToUniversalTime(), new DateTime(2003,4,4).ToUniversalTime()]);
        expectedErrors.Add([new DateTime(2002,4,4).ToUniversalTime(), new DateTime(2001,4,4).ToUniversalTime()]);
        expectedErrors.Add([new DateTime(2000,4,4).ToUniversalTime(), new DateTime(1950,4,4).ToUniversalTime()]);
        expectedErrors.Add([new DateTime(1948,4,4).ToUniversalTime()]);
        expectedErrors.Add([]);
        
        var store = new TestErrorStore(Factory);
        for (var pageIndex = 0; pageIndex <= 4; pageIndex++)
        {
            var errors = store
                .GetErrorsForPage(pageIndex, 2)
                .Select(e => e.Time)
                .ToArray();
            Assert.That(errors, Is.EquivalentTo(expectedErrors[pageIndex]));
        }
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
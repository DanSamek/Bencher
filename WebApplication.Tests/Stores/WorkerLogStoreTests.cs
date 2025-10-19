using WebApplication.Stores;
using WebApplication.Tests.Builders;
using WebApplication.Data.Models;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class WorkerLogStoreTests : TestBase
{
    /// <summary>
    /// Tests for <see cref="WorkerLogStore.AddError" />.
    /// </summary>
    [Test]
    public void AddError()
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
                        .AddWorker(1)
                    .Close()
                .Close()
            .Close()
        .Close();
        
        using var context = Factory.CreateDbContext();
        var wl = context.WorkerLogs.First();
        var test = context.Tests.First();

        var error = new Error
        {
            Time = DateTime.UtcNow,
            Log =
            [
                0x10,
                0x11,
                0x12,
                0x13
            ],
            Test = test,
            WorkerLog = wl
        };
        
        var store = new WorkerLogStore(Factory);
        store.AddError(wl, error);
        
        var errors = context.Errors.ToList();
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors, Is.Not.Empty);
        Assert.That(errors[0].Log, Is.EquivalentTo(new byte[] {0x10, 0x11, 0x12, 0x13}));
        Assert.That(errors[0].Time.Year, Is.EqualTo(error.Time.Year));
    }

    /// <summary>
    /// Tests for <see cref="WorkerLogStore.GetByConnectionId" />.
    /// </summary>
    [Test]
    public void GetByConnectionId()
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
            .AddWorker(1)
            .Close()
            .Close()
            .Close()
            .Close();

        var store = new WorkerLogStore(Factory);
        var wl = Factory.CreateDbContext().WorkerLogs.First();
        var workerLog = store.GetByConnectionId(wl.Id);
        
        Assert.That(workerLog, Is.Not.Null);
        Assert.That(workerLog.Id, Is.EqualTo(wl.Id));
    }
}
using Microsoft.EntityFrameworkCore;
using WebApplication.Data.Models;
using WebApplication.Stores;
using WebApplication.Tests.Builders;

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
        var wlId = context.WorkerLogs.First().Id;
        var year = DateTime.UtcNow.Year;
     
        var store = new WorkerLogStore(Factory);
        var workerLog = store.GetByConnectionId(wlId);
        Assert.That(workerLog, Is.Not.Null);
        store.AddError(workerLog,[0x10, 0x11, 0x12, 0x13 ]);
        
        var errors = context.TestErrors
            .Include(e => e.Log)
            .Include(e => e.Test)
            .ToList();
        
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors, Is.Not.Empty);
        Assert.That(errors,Has.Count.EqualTo(1));
        Assert.That(errors[0].Test.Name, Is.EqualTo("test_1"));
        Assert.That(errors[0].Log.Data, Is.EquivalentTo(new byte[] {0x10, 0x11, 0x12, 0x13}));
        Assert.That(errors[0].Time.Year, Is.EqualTo(year));
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

    /// <summary>
    /// Tests <see cref="WorkerLogStore.StopAllWorkers"/>.
    /// </summary>
    [Test]
    public async Task StopAllWorkers()
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
                        .AddWorker(2)
                        .AddWorker(4)
                        .AddWorker(8)
                        .AddWorker(1)
                        .Close()
                    .Close()
                .Close()
            .Close();

        var store = new WorkerLogStore(Factory);
        var wl = Factory.CreateDbContext().WorkerLogs.First();
        var workerLog = store.GetByConnectionId(wl.Id)!;

        var runningWorkers = Factory.CreateDbContext()
            .WorkerLogs
            .Where(wl => wl.Test.Id == workerLog.Test.Id && wl.State == WorkerLogState.Active)
            .ToArray();
        
        Assert.That(runningWorkers, Has.Length.EqualTo(5));
        await store.StopAllWorkers(workerLog.Test.Id);
        
        var stoppedWorkers = Factory.CreateDbContext()
            .WorkerLogs
            .Where(wl => wl.Test.Id == workerLog.Test.Id && wl.State == WorkerLogState.Finished)
            .ToArray();
        
        Assert.That(stoppedWorkers, Has.Length.EqualTo(5));
    }
}
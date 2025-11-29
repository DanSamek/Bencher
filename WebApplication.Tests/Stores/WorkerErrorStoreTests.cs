using Microsoft.EntityFrameworkCore;
using WebApplication.Stores;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class WorkerErrorStoreTests : TestBase
{
    /// <summary>
    /// Tests <see cref="WorkerErrorStore.AddError" />.
    /// </summary>
    [Test]
    public void AddError()
    {
        var store = new WorkerErrorStore(Factory);
        var data = new byte[] { 0x11, 0x22, 0x33 };
        store.AddError(data);
        
        var errors = Factory.CreateDbContext()
            .WorkerErrors
            .Include(error => error.Log)
            .ToArray();
        
        Assert.That(errors, Has.Length.EqualTo(1));
        Assert.That(errors[0].Log.Data, Is.EqualTo(data));
    }
}
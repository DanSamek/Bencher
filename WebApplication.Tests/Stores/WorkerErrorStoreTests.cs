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
    
    /// <summary>
    /// Tests <see cref="WorkerErrorStore.GetErrors" />.
    /// </summary>
    [Test]
    public void GetErrors()
    {
        var store = new WorkerErrorStore(Factory);
        var data = new byte[] { 0x11, 0x22, 0x33 };
        
        store.AddError(data);
        var errors = store.GetErrors();
        
        Assert.That(errors, Has.Count.EqualTo(1));
    }
    
    /// <summary>
    /// Tests <see cref="WorkerErrorStore.LoadContent" />.
    /// </summary>
    [Test]
    public void LoadContent()
    {
        var store = new WorkerErrorStore(Factory);
        var data = new byte[] { 0x11, 0x22, 0x33 };
        store.AddError(data);
        var id = Factory.CreateDbContext().WorkerErrors.First().Id;
        var content = store.LoadContent(id);
        
        Assert.That(content, Is.EqualTo(data));
    }
}
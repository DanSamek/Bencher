using Microsoft.EntityFrameworkCore;
using WebApplication.Stores;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class WorkerErrorStoreTests : TestBase
{
    /// <summary>
    /// Tests <see cref="WorkerErrorStore.GetErrorsForPage"/> with page = 2;
    /// </summary>
    [Test]
    public void GetErrorsForPage()
    {
        var dateTimes = new DateTime[]
        {
            new DateTime(2000,1,4).ToUniversalTime(),
            new DateTime(2001,1,4).ToUniversalTime(),
            new DateTime(2002,1,4).ToUniversalTime(),
            new DateTime(2003,1,4).ToUniversalTime(),
            new DateTime(1995,1,4).ToUniversalTime(),
            new DateTime(1996,1,4).ToUniversalTime(),
            new DateTime(2015,1,4).ToUniversalTime()
        };
        var data = new byte[] { 0x11, 0x22, 0x33 };
        var tmpStore = new WorkerErrorStore(Factory);
        
        foreach (var dateTime in dateTimes) tmpStore.AddError(data);

        using var context = Factory.CreateDbContext();
        var workerErrors = context.WorkerErrors.ToArray();
        for (var i = 0; i < dateTimes.Length; i++)
        {
            workerErrors[i].Time = dateTimes[i];
        }
        context.SaveChanges();
        
        var sorted = dateTimes
            .OrderByDescending(x => x)
            .ToList();
        
        var expectedErrors = new List<DateTime[]>();
        expectedErrors.Add([sorted[0], sorted[1]]);
        expectedErrors.Add([sorted[2], sorted[3]]);
        expectedErrors.Add([sorted[4], sorted[5]]);
        expectedErrors.Add([sorted[6]]);
        expectedErrors.Add([]);
        
        var store = new WorkerErrorStore(Factory);
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
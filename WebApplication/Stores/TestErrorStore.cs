using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class TestErrorStore : Store<TestError>
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestErrorStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory){}

    /// <inheritdoc /> 
    protected override DbSet<TestError> GetDbSet() => Context.TestErrors;
    
    /// <summary>
    /// Loads test error content.
    /// </summary>
    public byte[] LoadContent(int testErrorId)
        => GetDbSet()
            .Include(ob => ob.Log)
            .Where(ob => ob.Id == testErrorId)
            .Select(ob => ob.Log.Data)
            .First();
    
    /// <summary>
    /// Returns all errors, that occured in the test ordered by time - last will be first.
    /// </summary>
    /// <param name="testId">Id of the test</param>
    public IReadOnlyList<TestError> GetErrorsForTest(int testId)
        => OrderedByTimeWithTest()
            .Where(e => e.Test.Id == testId)
            .ToArray();
    
    /// <summary>
    /// Returns errors for the paging ordered by time - the last will be the first.
    /// ! Without log.
    /// </summary>
    public IReadOnlyList<TestError> GetErrorsForPage(int pageIndex, int pageSize = WebConstants.PAGE_SIZE)
        => OrderedByTimeWithTest()
            .TakePage(pageIndex, pageSize)
            .ToArray();
    
    private IOrderedQueryable<TestError> OrderedByTimeWithTest()
        => GetDbSet()
            .Include(e => e.Test)
            .OrderByDescending(t => t.Time);
}
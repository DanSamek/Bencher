using System.Numerics;
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
    /// Returns all errors ordered by time - last will be first.
    /// ! Without data.
    /// </summary>
    public IReadOnlyList<TestError> GetErrors()
        => OrderedByTimeWithTest()
            .ToArray();
    
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
    
    private IOrderedQueryable<TestError> OrderedByTimeWithTest()
        => GetDbSet()
            .Include(e => e.Test)
            .OrderByDescending(t => t.Time);
}
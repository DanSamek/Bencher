using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class OpeningBookStore : Store<OpeningBook>
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public OpeningBookStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) {}
    
    /// <inheritdoc /> 
    protected override DbSet<OpeningBook> GetDbSet() => Context.OpeningBooks;
    
    /// <summary>
    /// Returns all opening books for the user's id. [not tracked]
    /// </summary>
    public IReadOnlyList<OpeningBook> GetOpeningBooksForUser(string userId)
    {
        var openingBooks = GetDbSet()
            .AsNoTracking()
            .Where(ob => ob.User != null && ob.User.Id == userId)
            .ToArray();
        
        return openingBooks;
    }
    
    /// <summary>
    /// Returns all shared opening books.
    /// </summary>
    public IReadOnlyList<OpeningBook> GetSharedOpeningBooks()
    {
        var openingBooks = GetDbSet()
            .AsNoTracking()
            .Where(ob => ob.User == null)
            .ToArray();
        
        return openingBooks;
    }
    
    /// <summary>
    /// Adds an opening book.
    /// </summary>
    public void Add(string userId, string name, byte[] data, int depth, OpeningBookType openingBookType)
    {
        var user = Context.Users
            .FirstOrDefault(u => u.Id == userId);
        
        var openingBook = new OpeningBook
        {
            Name = name,
            Type = openingBookType,
            Depth = depth,
            Tests = [],
            User = user
        };
        
        Add(openingBook);
        openingBook.Data = new OpeningBookContent
        {
            Data = data,
            OpeningBookId = openingBook.Id
        };
        Update(openingBook);
    }
    
    /// <summary>
    /// Removes opening book by id.
    /// </summary>
    public void DeleteById(int openingBookId)
        => GetDbSet()
            .Where(ob => ob.Id == openingBookId)
            .ExecuteDelete();
    
    /// <summary>
    /// Checks if opening book is used in any running test.
    /// </summary>
    public bool AnyRunningTest(int openingBookId)
        => Context
            .Tests
            .Any(t => t.OpeningBook.Id == openingBookId && t.State != TestState.Stopped &&
                        t.State != TestState.Finished);
    
    /// <summary>
    /// Loads opening book content.
    /// </summary>
    public byte[] LoadContent(int openingBookId)
    {
        var data = GetDbSet()
            .Include(ob => ob.Data)
            .Where(ob => ob.Id == openingBookId)
            .Select(ob => ob.Data)
            .First();
        
        return data.Data;
    }
    
}
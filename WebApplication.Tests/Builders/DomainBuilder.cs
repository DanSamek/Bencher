using Shared;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Tests.Builders;


public class DomainBuilder
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// .Ctor
    /// </summary>
    public DomainBuilder(ApplicationDbContext context)
    {
        _context = context;
    }

    public UserBuilder CreateUser(string username)
    {
        var user = new ApplicationUser
        {
            Tests = [],
            OpeningBooks = [],
            UserName = username,
            PasswordHash = "123456789",
            Email = $"{username}@test.com",
            Engines = []
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        var userBuilder = new UserBuilder(user, _context, this);

        return userBuilder;
    }

    public DomainBuilder CreateBook(string name, byte[]? content = null)
    {
        var book = new OpeningBook
        {
            Name = name,
            Type = OpeningBookType.EPD,
            Tests = []
        };
        
        _context.OpeningBooks.Add(book);
        _context.SaveChanges();
        
        book.Data = new OpeningBookContent()
        {
            OpeningBookId = book.Id,
            Data = content ?? [0x69],
        };  
        
        _context.OpeningBooks.Update(book);
        _context.SaveChanges();
        
        return this;
    }

    public DomainBuilder CreateBook(string name, string username)
    {
        var user = _context.Users.First(u => u.UserName == username);
        var book = new OpeningBook
        {
            Name = name,
            Type = OpeningBookType.EPD,
            Tests = [],
            User = user
        };
        
        _context.OpeningBooks.Add(book);
        _context.SaveChanges();
        
        book.Data = new OpeningBookContent()
        {
            OpeningBookId = book.Id,
            Data = [0x69],
        };
        
        
        _context.OpeningBooks.Update(book);
        _context.SaveChanges();
        return this;
    }
    

    public DomainBuilder CreateSprtSettings(double elo0 = 0, double elo1 = 2)
    {
        var settings = new SprtSettings
        {
            Elo0 = elo0,
            Elo1 = elo1,
            Alpha = 0.05,
            Beta = 0.05,
            Tests = []
        };
        
        _context.SprtSettings.Add(settings);
        _context.SaveChanges();
        return this;
    }
    
    public void Close(){ }
}

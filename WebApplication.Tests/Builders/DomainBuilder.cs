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

    public DomainBuilder CreateBook(string name)
    {
        var book = new OpeningBook
        {
            Name = name,
            Data = [0x69],
            Type = OpeningBookType.EPD,
            Depth = 0,
            Tests = []
        };
        
        _context.OpeningBooks.Add(book);
        _context.SaveChanges();
        
        return this;
    }

    public DomainBuilder CreateBook(string name, string username)
    {
        var user = _context.Users.First(u => u.UserName == username);
        var book = new OpeningBook
        {
            Name = name,
            Data = [0x69],
            Type = OpeningBookType.EPD,
            Depth = 0,
            Tests = [],
            User = user
        };
        
        _context.OpeningBooks.Add(book);
        _context.SaveChanges();
        return this;
    }
    

    public DomainBuilder CreateSprtSettings()
    {
        var settings = new SprtSettings
        {
            Elo0 = 0,
            Elo1 = 2,
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

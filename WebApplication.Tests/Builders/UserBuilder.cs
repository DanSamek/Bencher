using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Tests.Builders;

public class UserBuilder
{
    private readonly ApplicationUser _user;
    private readonly ApplicationDbContext _context;
    private readonly DomainBuilder _domainBuilder;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public UserBuilder(ApplicationUser user, ApplicationDbContext context, DomainBuilder domainBuilder)
    {
        _user = user;
        _context = context;
        _domainBuilder = domainBuilder;
    }

    public EngineBuilder AddEngine(string name)
    {
        var engine = new Engine
        {
            Name = name,
            GitUrl = $"git-url{name}",
            BuildScript = [],
            User = _user,
            Tests = [],
            Branches = []
        };
        _user.Engines.Add(engine);
        _context.Add(engine);
        _context.SaveChanges();
        var engineBuilder = new EngineBuilder(engine, _context, this, _user);
        return engineBuilder;
    }

    public UserBuilder WithAccessToken(string? accessToken)
    {
        if (accessToken is null) return this;
        
        _user.AccessToken = accessToken;
        _context.Users.Update(_user);
        _context.SaveChanges();
        return this;
    }

    public DomainBuilder Close()
    {
        _context.SaveChanges();
        return _domainBuilder;
    }
}
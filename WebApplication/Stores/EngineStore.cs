using System.Text;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class EngineStore : Store<Engine>
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public EngineStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) {}
    
    /// <inheritdoc /> 
    protected override DbSet<Engine> GetDbSet()
    {
        return Context.Engines;
    }

    /// <summary>
    /// Returns all engines for the user's id. [not tracked]
    /// </summary>
    public List<Engine> GetEnginesForUser(string userId)
    {
        var engines = GetDbSet()
            .AsNoTracking()
            .Where(e => e.User.Id == userId)
            .ToList();
        
        return engines;
    }
    
    /// <summary>
    /// Adds an engine.
    /// Note, it can throw, if user doesn't exist.
    /// </summary>
    public void Add(string name, string gitUrl, string buildScript, string userId)
    {
        var user = Context.Users
            .First(u => u.Id == userId);
        
        var buildScriptBytes = Engine.GetBuildScriptBytes(buildScript);
        
        var engine = new Engine
        {
            Name = name,
            GitUrl = gitUrl,
            BuildScript = buildScriptBytes,
            User = user,
            Tests = [],
            Branches = []
        };

        GetDbSet().Add(engine);
        SaveChanges();
    }
    
    /// <summary>
    /// Removes engine by the id. 
    /// </summary>
    public void DeleteById(int id)
    {
        GetDbSet().Where(e => e.Id == id).ExecuteDelete();
    }

    /// <summary>
    /// Updates an engine.
    /// </summary>
    public void Update(int engineId, string name, string gitUrl, string buildScript)
    {
        var bytes = Engine.GetBuildScriptBytes(buildScript);
        GetDbSet()
            .Where(e => e.Id == engineId)
            .ExecuteUpdateAsync(spc =>
                spc.SetProperty(t => t.Name, name)
                    .SetProperty(t => t.GitUrl, gitUrl)
                    .SetProperty(t => t.BuildScript, bytes)
            );
    }

    /// <summary>
    /// Check if any test is running for the engine. 
    /// </summary>
    public bool AnyRunningTest(int engineId)
    {
        var result = Context.Tests
            .Any(t => t.State == TestState.Running || t.State == TestState.Autobenched);
        return result;
    }
}
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class SprtSettingsStore : Store<SprtSettings>
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public SprtSettingsStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory){}

    /// <inheritdoc /> 
    protected override DbSet<SprtSettings> GetDbSet() => Context.SprtSettings;

    /// <summary>
    /// Creates a sprt settings if there is no existing with same property values.
    /// </summary>
    public SprtSettings GetExistingSprtSettingsOrCreate(double elo0, double elo1, double alpha, double beta)
    {
        var result = GetDbSet()
            .FirstOrDefault(x => Math.Abs(x.Alpha - alpha) < double.Epsilon 
                                            && Math.Abs(x.Beta - beta) < double.Epsilon 
                                            && Math.Abs(x.Elo0 - elo0) < double.Epsilon 
                                            && Math.Abs(x.Elo1 - elo1) < double.Epsilon);
        
        if (result != null)
        {
            return result;
        }
        
        var settings = new SprtSettings
        {
            Elo0 = elo0,
            Elo1 = elo1,
            Alpha = alpha,
            Beta = beta,
            Tests = []
        };

        result = GetDbSet().Add(settings).Entity;
        SaveChanges();
        return result;
    }
}
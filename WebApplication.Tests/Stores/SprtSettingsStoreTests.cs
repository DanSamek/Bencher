using WebApplication.Stores;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class SprtSettingsStoreTests : TestBase
{
    /// <summary>
    /// Tests <see cref="SprtSettingsStore.GetExistingSprtSettingsOrCreate"/>.
    /// </summary>
    [TestCase(1,1,0.54,0.54,
        1,1,0.54,0.54,
        1)]
    [TestCase(1,1.1,0.54,0.54,
        1,1.1,0.55,0.54,
        2)]
    [TestCase(1,1,0.54,0.54,
        1,1,0.54,0.55,
        2)]
    [TestCase(1,1,0.54,0.54,
        1,2,0.54,0.54,
        2)]
    [TestCase(1,1,0.54,0.54,
        2,1,0.54,0.54,
        2)]
    [TestCase(1.1,1,0.54,0.54,
        1.1,1,0.54,0.54,
        1)]
    [TestCase(2,1,0.555,0.532,
        2,1,0.555,0.532,
        1)]
    public void GetExistingSprtSettingsOrCreate(double elo0, double elo1, double alpha, double beta, 
                                                      double elo0_2, double elo1_2, double alpha_2, double beta_2, 
                                                      int expectedCount)
    {
        var store = new SprtSettingsStore(Factory);
        store.GetExistingSprtSettingsOrCreate(elo0, elo1,  alpha, beta);
        Assert.That(Factory.CreateDbContext().SprtSettings.Count(), Is.EqualTo(1));
        
        store.GetExistingSprtSettingsOrCreate(elo0_2, elo1_2, alpha_2, beta_2);
        Assert.That(Factory.CreateDbContext().SprtSettings.Count(), Is.EqualTo(expectedCount));
    }
}
using System.Text;
using WebApplication.Data.Models;
using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]   
public class EngineStoreTests : TestBase
{
    /// <summary>
    /// Test for <see cref="EngineStore.DeleteById"/>
    /// </summary>
    [Test]
    public void DeleteById()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateUser("test_user")
                .AddEngine("test_engine")
                .Close()
            .Close()
        .Close();

        var store = new EngineStore(Factory);
        var testId = Factory.CreateDbContext().Engines.First().Id;
        store.DeleteById(testId);
        Assert.That(Factory.CreateDbContext().Engines.Count(), Is.EqualTo(0));
    }
    
    /// <summary>
    /// Test for <see cref="EngineStore.GetEnginesForUser"/>
    /// </summary>
    [Test]
    public void GetEnginesForUser()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateUser("test_user")
                .AddEngine("test_engine")
                .Close()
                .AddEngine("test_engine_2")
                .Close()
            .Close()
            .CreateUser("test_user_2")
                .AddEngine("test_engine_3")
                .Close()
            .Close()
        .Close();

        var userId =  Factory.CreateDbContext().Users.First().Id; 
        
        var store = new EngineStore(Factory);
        var engines = store.GetEnginesForUser(userId);
        
        Assert.That(engines, Is.Not.Null);
        Assert.That(engines, Has.Count.EqualTo(2));
        Assert.That(engines[0].Name, Is.EqualTo("test_engine"));
        Assert.That(engines[1].Name, Is.EqualTo("test_engine_2"));
    }

    /// <summary>
    /// Test for <see cref="EngineStore.Add(string, string, string, string)"/>
    /// </summary>
    [Test]
    public void Add()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateUser("test_user")
            .Close()
        .Close();
        
        var store = new EngineStore(Factory);
        
        var userId = Factory.CreateDbContext().Users.First().Id;
        store.Add("stockfish", "https://github.com/DanSamek/Stockfish", "make -j profile-build", userId);
        
        Assert.That(Factory.CreateDbContext().Engines.Count(), Is.EqualTo(1));
        var test = Factory.CreateDbContext().Engines.First();
        
        Assert.That(test.Name, Is.EqualTo("stockfish"));
        Assert.That(test.GitUrl, Is.EqualTo("https://github.com/DanSamek/Stockfish"));
        Assert.That(Encoding.ASCII.GetString(test.BuildScript), Is.EqualTo("make -j profile-build"));
    }
    
    /// <summary>
    /// Tests for <see cref="EngineStore.Update(int, string , string, string)" />.
    /// </summary>
    [Test]
    public void Update()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateUser("test_user")
                .AddEngine("test_engine")
            .Close();
        
        var store = new EngineStore(Factory);
        var engineId = Factory.CreateDbContext().Engines.First().Id;
        store.Update(engineId, "test", "https://test.com","build - script");

        var engine = Factory.CreateDbContext().Engines.First();
        Assert.That(engine.Name, Is.EqualTo("test"));
        Assert.That(engine.GitUrl, Is.EqualTo("https://test.com"));
        Assert.That(engine.GetBuildScriptString(), Is.EqualTo("build - script"));
    }
    
    /// <summary>
    /// Tests for <see cref="EngineStore.AnyNotFinishedTest" />.
    /// </summary>
    [TestCase(TestState.Paused, true)]
    [TestCase(TestState.Autobenched, true)]
    [TestCase(TestState.Running, true)]
    [TestCase(TestState.Finished, false)]
    [TestCase(TestState.Stopped, false)]
    public void AnyNotFinishedTest_SomeRunning(TestState testState, bool expected)
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateSprtSettings()
            .CreateBook("test_book")
            .CreateUser("test_user")
                .AddEngine("stockfish")
                    .AddBranch("base_branch")
                    .AddBranch("test_branch")
                        .AddTest("test_1", "test_book", "base_branch", "test_branch", state: testState)
                        .Close()
                    .Close()
                .Close()
            .Close();

        var store = new EngineStore(Factory);
        var engineId = Factory.CreateDbContext().Engines.First().Id;
        var result = store.AnyNotFinishedTest(engineId);
        
        Assert.That(result, Is.EqualTo(expected));
    }
}
using Moq;
using Worker.Notifier;
using Worker.TestProcessors.GameTestProcessor;

namespace Worker.Tests.TestProcessors.GameTestProcessor;

[TestFixture]
public class TestStateWatcherTests
{
    /// <summary>
    /// Tests <see cref="TestStateWatcher.Watch"/>
    ///     - We expect that test is running and after another period test is stopped.
    /// </summary>
    [Test]
    public void Watch()
    {
        var notifierMock = new Mock<INotifier>();
        notifierMock.SetupSequence(n => n.IsTestStillRunning(5))
            .Returns(true)
            .Returns(false);
        var testStateWatcher = new TestStateWatcher(notifierMock.Object, 5, new TimeSpan(0,0,2));
        
        Task.Run(() => testStateWatcher.Watch());
        
        Assert.That(testStateWatcher.Running, Is.True);
        
        Thread.Sleep(3000); // not ideal D:
        Assert.That(testStateWatcher.Running, Is.False);
        
        testStateWatcher.EnsureStopped();
    }
    
}
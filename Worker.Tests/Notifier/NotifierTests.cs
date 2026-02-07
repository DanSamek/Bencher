using Moq;
using Shared.Dtos.Responses;
using Worker.Notifier;

namespace Worker.Tests.Notifier;

[TestFixture]
public class NotifierTests
{
    /// <summary>
    /// Tests <see cref="INotifier.AddNotifyRunningTest"/>.
    /// </summary>
    [Test]
    public async Task AddNotifyRunningTest()
    {
        var communicationMock = new Mock<ICommunication>();
        communicationMock.Setup(c => c.RunningTest(5)).Returns(new RunningTestResponseDto(true));
        var notifier = new Worker.Notifier.Notifier(communicationMock.Object);
        
        await notifier.AddNotifyRunningTest(5);
        communicationMock.Verify(c => c.RunningTest(5), Times.Once);
        notifier.EnsureStopped();
    }

    /// <summary>
    /// Tests <see cref="INotifier.RemoveNotifyRunningTest"/>.
    /// </summary>
    [Test]
    public async Task RemoveNotifyRunningTest()
    {
        var communicationMock = new Mock<ICommunication>();
        communicationMock.Setup(c => c.RunningTest(5))
            .Returns(new RunningTestResponseDto(true));
        var notifier = new Worker.Notifier.Notifier(communicationMock.Object);
        
        await notifier.AddNotifyRunningTest(5);
        await notifier.RemoveNotifyRunningTest(5);
        
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(() => notifier.Run()) ;
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Thread.Sleep(1000);
        notifier.EnsureStopped();
        
        communicationMock.Verify(c => c.RunningTest(5), Times.Once);
    }

    /// <summary>
    /// Tests <see cref="INotifier.IsTestStillRunning"/>.
    /// </summary>
    [Test]
    public async Task IsTestStillRunning()
    {
        var communicationMock = new Mock<ICommunication>();
        communicationMock.SetupSequence(c => c.RunningTest(5))
            .Returns(new RunningTestResponseDto(true))
            .Returns(new RunningTestResponseDto(true))
            .Returns(new RunningTestResponseDto(false));
        var notifier = new Worker.Notifier.Notifier(communicationMock.Object, new TimeSpan(0,0,2));

        var running = notifier.IsTestStillRunning(5);
        Assert.That(running, Is.True);
        
        await notifier.AddNotifyRunningTest(5);
        communicationMock.Verify(c => c.RunningTest(5), Times.Once);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(() => notifier.Run());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Thread.Sleep(500);
        
        running = notifier.IsTestStillRunning(5);
        Assert.That(running, Is.True);
        communicationMock.Verify(c => c.RunningTest(5), Times.Exactly(2));
        Thread.Sleep(2000);
        
        running = notifier.IsTestStillRunning(5);
        Assert.That(running, Is.False);
        communicationMock.Verify(c => c.RunningTest(5), Times.AtLeast(3));
        notifier.EnsureStopped();
    }
}
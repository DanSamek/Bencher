using System.Diagnostics;
using Moq;
using Worker.Dependencies;
using Worker.ProcessOperations;

namespace Worker.Tests.Dependencies;

[TestFixture]
public class GitDependencyTests
{
    /// <summary>
    /// Tests <see cref="GitDependency.Validate"/> success path.
    /// </summary>
    [Test]
    public void Validation_Success()
    {
        var processRunnerMock = new Mock<IProcessRunner>();
        processRunnerMock.Setup(m => m.RunProcess(It.IsAny<ProcessStartInfo>())).Returns(new RunResult
        (
            "git version 2.39.5",
            ""
        ));
        var dependency = new GitDependency(processRunnerMock.Object, new ProcessStartInfoCreator()); 
        var result = dependency.Validate();
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests <see cref="GitDependency.Validate"/> error path.
    /// </summary>
    [TestCase("bash: git: command not found")]
    public void Validation_Failure(string processOutput)
    {
        var processRunnerMock = new Mock<IProcessRunner>();
        processRunnerMock.Setup(m => m.RunProcess(It.IsAny<ProcessStartInfo>())).Returns(new RunResult
        (
            "", 
            processOutput
        ));
        var dependency = new GitDependency(processRunnerMock.Object, new ProcessStartInfoCreator()); 
        var result = dependency.Validate();
        Assert.That(result, Is.False);
    }
    
}
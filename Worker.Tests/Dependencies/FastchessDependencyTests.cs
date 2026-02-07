using System.Diagnostics;
using Moq;
using Worker.Dependencies;
using Worker.ProcessOperations;

namespace Worker.Tests.Dependencies;

[TestFixture]
public class FastchessDependencyTests
{
    /// <summary>
    /// Tests <see cref="FastchessDependency.Validate"/> success path - fastchess file exists. 
    /// </summary>
    [Test]
    public void Validation_Success()
    {
        Directory.CreateDirectory("/tmp/bencher-worker");
        File.Create("/tmp/bencher-worker/fastchess").Close();
        var dependency = new FastchessDependency(new ProcessRunner(), new ProcessStartInfoCreator());
        var result = dependency.Validate();
        Assert.That(result, Is.True);
        Directory.Delete("/tmp/bencher-worker", true);
    }
    
    /// <summary>
    /// Tests <see cref="FastchessDependency.Validate"/> error path - fastchess file doesn't exist.
    /// </summary>
    [Test]
    public void Validation_Failure()
    {
        var dependency = new FastchessDependency(new ProcessRunner(), new ProcessStartInfoCreator());
        var result = dependency.Validate();
        Assert.That(result, Is.False);
    }
    
    /// <summary>
    /// Tests <see cref="FastchessDependency.TryResolve"/> success path - its possible to resolve fastchess dependency - build 
    /// </summary>
    [Test]
    public void TryResolve_Success()
    {
         var processRunnerMock = new Mock<IProcessRunner>();
         processRunnerMock.Setup(m => m.RunProcess(It.IsAny<ProcessStartInfo>())).Returns(new RunResult
         (
             "",
             ""
         ));
         processRunnerMock.Setup(m => m.RunProcess(It.Is<ProcessStartInfo>(x => x.Arguments.Contains("make -j CXX=clang++")))).Returns(new RunResult
         (
             "",
             "fastchess build"
         ));
        var dependency = new FastchessDependency(processRunnerMock.Object, new ProcessStartInfoCreator());
        var result = dependency.TryResolve(Compilers.Clang);
        Assert.That(result, Is.True);
    }
}
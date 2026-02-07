using System.Diagnostics;
using Moq;
using Worker.Dependencies;
using Worker.ProcessOperations;

namespace Worker.Tests.Dependencies;

[TestFixture]
public class ClangDependencyTests
{
    /// <summary>
    /// Tests <see cref="ClangDependency.Validate"/> success path.
    /// </summary>
    [Test]
    public void Validation_Success()
    {
        var processRunnerMock = new Mock<IProcessRunner>();
        processRunnerMock.Setup(m => m.RunProcess(It.IsAny<ProcessStartInfo>())).Returns(new RunResult
        (
            "",
            """
            Debian clang version 14.0.6
            Target: x86_64-pc-linux-gnu
            Thread model: posix
            InstalledDir: /usr/bin
            Found candidate GCC installation: /usr/bin/../lib/gcc/x86_64-linux-gnu/12
            Selected GCC installation: /usr/bin/../lib/gcc/x86_64-linux-gnu/12
            Candidate multilib: .;@m64
            Candidate multilib: 32;@m32
            Candidate multilib: x32;@mx32
            Selected multilib: .;@m64
            Found CUDA installation: /usr/local/cuda-12.6, version 
            """
        ));
        var dependency = new ClangDependency(processRunnerMock.Object, new ProcessStartInfoCreator()); 
        var result = dependency.Validate();
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests <see cref="ClangDependency.Validate"/> error path.
    /// </summary>
    [TestCase("bash: clang: command not found")]
    [TestCase("Target: x86_64-pc-linux-gnu\nThread model: posix\nInstalledDir: /usr/bin\nFound candidate GCC installation: /usr/bin/../lib/gcc/x86_64-linux-gnu/12\nSelected GCC installation: /usr/bin/../lib/gcc/x86_64-linux-gnu/12\nCandidate multilib: .;@m64\nCandidate multilib: 32;@m32\nCandidate multilib: x32;@mx32\nSelected multilib: .;@m64\nFound CUDA installation: /usr/local/cuda-12.6, version ")]
    public void Validation_Failure(string processOutput)
    {
        var processRunnerMock = new Mock<IProcessRunner>();
        processRunnerMock.Setup(m => m.RunProcess(It.IsAny<ProcessStartInfo>())).Returns(new RunResult
        (
            "", 
            processOutput
        ));
        var dependency = new ClangDependency(processRunnerMock.Object, new ProcessStartInfoCreator()); 
        var result = dependency.Validate();
        Assert.That(result, Is.False);
    }
}
using System.Diagnostics;
using Moq;
using Worker.Dependencies;
using Worker.ProcessOperations;

namespace Worker.Tests.Dependencies;

[TestFixture]
public class GCCDependencyTests
{
    /// <summary>
    /// Tests <see cref="GCCDependency.Validate"/> success path.
    /// </summary>
    [Test]
    public void Validation_Success()
    {
        var processRunnerMock = new Mock<IProcessRunner>();
        processRunnerMock.Setup(m => m.RunProcess(It.IsAny<ProcessStartInfo>())).Returns(new RunResult
        (
            "",
            """
            Using built-in specs.
            COLLECT_GCC=gcc
            COLLECT_LTO_WRAPPER=/usr/lib/gcc/x86_64-linux-gnu/12/lto-wrapper
            OFFLOAD_TARGET_NAMES=nvptx-none:amdgcn-amdhsa
            OFFLOAD_TARGET_DEFAULT=1
            Target: x86_64-linux-gnu
            Configured with: ../src/configure -v --with-pkgversion='Debian 12.2.0-14+deb12u1' --with-bugurl=file:///usr/share/doc/gcc-12/README.Bugs --enable-languages=c,ada,c++,go,d,fortran,objc,obj-c++,m2 --prefix=/usr --with-gcc-major-version-only --program-suffix=-12 --program-prefix=x86_64-linux-gnu- --enable-shared --enable-linker-build-id --libexecdir=/usr/lib --without-included-gettext --enable-threads=posix --libdir=/usr/lib --enable-nls --enable-clocale=gnu --enable-libstdcxx-debug --enable-libstdcxx-time=yes --with-default-libstdcxx-abi=new --enable-gnu-unique-object --disable-vtable-verify --enable-plugin --enable-default-pie --with-system-zlib --enable-libphobos-checking=release --with-target-system-zlib=auto --enable-objc-gc=auto --enable-multiarch --disable-werror --enable-cet --with-arch-32=i686 --with-abi=m64 --with-multilib-list=m32,m64,mx32 --enable-multilib --with-tune=generic --enable-offload-targets=nvptx-none=/build/reproducible-path/gcc-12-12.2.0/debian/tmp-nvptx/usr,amdgcn-amdhsa=/build/reproducible-path/gcc-12-12.2.0/debian/tmp-gcn/usr --enable-offload-defaulted --without-cuda-driver --enable-checking=release --build=x86_64-linux-gnu --host=x86_64-linux-gnu --target=x86_64-linux-gnu
            Thread model: posix
            Supported LTO compression algorithms: zlib zstd
            gcc version 12.2.0 (Debian 12.2.0-14+deb12u1) 
            """
        ));
        var dependency = new GCCDependency(processRunnerMock.Object, new ProcessStartInfoCreator()); 
        var result = dependency.Validate();
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests <see cref="GCCDependency.Validate"/> error path.
    /// </summary>
    [TestCase("bash: gcc: command not found")]
    [TestCase("Using built-in specs.\nCOLLECT_GCC=gcc\nCOLLECT_LTO_WRAPPER=/usr/lib/gcc/x86_64-linux-gnu/12/lto-wrapper\nOFFLOAD_TARGET_NAMES=nvptx-none:amdgcn-amdhsa\nOFFLOAD_TARGET_DEFAULT=1\nTarget: x86_64-linux-gnu\nConfigured with: ../src/configure -v --with-pkgversion='Debian 12.2.0-14+deb12u1' --with-bugurl=file:///usr/share/doc/gcc-12/README.Bugs --enable-languages=c,ada,c++,go,d,fortran,objc,obj-c++,m2 --prefix=/usr --with-gcc-major-version-only --program-suffix=-12 --program-prefix=x86_64-linux-gnu- --enable-shared --enable-linker-build-id --libexecdir=/usr/lib --without-included-gettext --enable-threads=posix --libdir=/usr/lib --enable-nls --enable-clocale=gnu --enable-libstdcxx-debug --enable-libstdcxx-time=yes --with-default-libstdcxx-abi=new --enable-gnu-unique-object --disable-vtable-verify --enable-plugin --enable-default-pie --with-system-zlib --enable-libphobos-checking=release --with-target-system-zlib=auto --enable-objc-gc=auto --enable-multiarch --disable-werror --enable-cet --with-arch-32=i686 --with-abi=m64 --with-multilib-list=m32,m64,mx32 --enable-multilib --with-tune=generic --enable-offload-targets=nvptx-none=/build/reproducible-path/gcc-12-12.2.0/debian/tmp-nvptx/usr,amdgcn-amdhsa=/build/reproducible-path/gcc-12-12.2.0/debian/tmp-gcn/usr --enable-offload-defaulted --without-cuda-driver --enable-checking=release --build=x86_64-linux-gnu --host=x86_64-linux-gnu --target=x86_64-linux-gnu\nThread model: posix\nSupported LTO compression algorithms: zlib zstd\n")]
    public void Validation_Failure(string processOutput)
    {
        var processRunnerMock = new Mock<IProcessRunner>();
        processRunnerMock.Setup(m => m.RunProcess(It.IsAny<ProcessStartInfo>())).Returns(new RunResult
        (
            "", 
            processOutput
        ));
        var dependency = new GCCDependency(processRunnerMock.Object, new ProcessStartInfoCreator()); 
        var result = dependency.Validate();
        Assert.That(result, Is.False);
    }
    
}
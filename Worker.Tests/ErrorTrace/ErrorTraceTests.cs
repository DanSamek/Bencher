using System.Text;
using Worker.UI;

namespace Worker.Tests.ErrorTrace;

using Worker;

[TestFixture]
public class ErrorTraceTests
{
    /// <summary>
    /// Tests <see cref="ErrorTrace.AddError"/>
    /// </summary>
    [Test]
    public void AddError()
    {
        var errorTrace = new UI.ErrorTrace();
        errorTrace.AddError("Something went wrong");
        
        Assert.That(errorTrace.Error(), Is.True);
        Assert.That(errorTrace.ToString(), Is.EqualTo("[ERROR]: Something went wrong\n"));
    }

    /// <summary>
    /// Tests <see cref="ErrorTrace.AddInfo"/>
    /// </summary>
    [Test]
    public void AddInfo()
    {
        var errorTrace = new UI.ErrorTrace();
        errorTrace.AddInfo("Some information");
        
        Assert.That(errorTrace.Error(), Is.False);
        Assert.That(errorTrace.ToString(), Is.EqualTo("[INFO]: Some information\n"));
    }
    
    /// <summary>
    /// Tests <see cref="ErrorTrace.AddInfoError"/>
    /// </summary>
    [Test]
    public void AddInfoError()
    {
        var errorTrace = new UI.ErrorTrace();
        errorTrace.AddInfoError("Some information", "Something went wrong");
        
        Assert.That(errorTrace.Error(), Is.True);
        Assert.That(errorTrace.ToString(), Is.EqualTo("[ERROR]: Something went wrong\n[INFO]: Some information\n"));
    }

    /// <summary>
    /// Tests <see cref="ErrorTrace.GetBytes"/>
    /// </summary>
    [Test]
    public void GetBytes()
    {
        var errorTrace = new UI.ErrorTrace();
        errorTrace.AddInfoError("Some information", "Something went wrong");

        var bytes = errorTrace.GetBytes();
        Assert.That(bytes, Is.Not.Null);

        var message = Encoding.UTF8.GetString(bytes);
        Assert.That(message, Is.EqualTo("[ERROR]: Something went wrong\n[INFO]: Some information\n"));
    }
}
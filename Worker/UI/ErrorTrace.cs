using System.Text;

namespace Worker.UI;

public class ErrorTrace
{
    private readonly StringBuilder _sb =  new StringBuilder();
    private bool _error;
    
    public bool Error() => _error;

    public void AddInfoError(string? info, string? error)
    { 
        AddError(error);
        AddInfo(info);
    }
    
    public void AddError(string? message)
        => _error |= Add(message, Helper.ERROR_PREFIX);

    public void AddInfo(string? message) 
        => Add(message, Helper.INFO_PREFIX);
    
    public override string ToString() 
        => _sb.ToString();
    
    public byte[] GetBytes() 
        => Encoding.UTF8.GetBytes(ToString());
    
    private bool Add(string? message, string prefix)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }
        var withPrefix = message.WithPrefix(prefix);
        _sb.AppendLine(withPrefix);
        
        Console.WriteLine(withPrefix);
        return true;
    }
}

namespace WebApplication.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Parses seconds from string of type "xxx+yyy", where seconds are xxx. 
    /// </summary>
    public static int Seconds(this string s)
    {
        var plusIndex = s.IndexOf('+');
        if (plusIndex == -1)
        {
            throw new Exception("String value is not in the expected format.");
        }

        var result = 0;
        var mult = 1;
        for (var i = plusIndex - 1; i >= 0; i--)
        {
            result += mult * (s[i] - '0');
            mult *= 10;
        }
        return result;
    }
}

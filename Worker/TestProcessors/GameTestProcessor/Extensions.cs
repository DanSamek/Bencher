using System.Text;
using Shared.Dtos.Responses;

namespace Worker.TestProcessors.GameTestProcessor;

public static class StringBuilderExtensions
{
    public static void AddOpeningBook(this StringBuilder builder, string openingBookPath, OpeningBookDto openingBookDto)
    {
        var openingBookType = openingBookDto.OpeningBookType;
        builder.AddArgument($"-openings order=random file={openingBookPath} format={openingBookType.ToString().ToLower()}");
    }
    
    public static void AddEngine(this StringBuilder sb, string cmdName)
    {
        sb.AddArgument($"-engine proto=uci {cmdName}");
    }

    public static void AddArgument(this StringBuilder sb, string argument)
    {   
        sb.Append($"{argument} ");
    }
}

public static class StringExtensions
{
    public static (int Seconds, decimal Increment) Tm(this string s)
    {
        var plusIndex = s.IndexOf('+');
        if (plusIndex == -1) throw new Exception("String value is not in the expected format.");
        
        var split = s.Split("+");
        return (int.Parse(split[0]), decimal.Parse(split[1]));
    }
}
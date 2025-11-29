using System.ComponentModel.DataAnnotations;

namespace Shared.CustomAttributes;

/// <summary>
/// Custom attribute for mac validations.
/// </summary>
public class MacAttribute : ValidationAttribute
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public MacAttribute(){}

    // 02:42:77:ba:b9:9b
    private const int MAC_CHAR_SIZE = 17;
    
    public override bool IsValid(object? value)
    {
        if (value is not string str) return false;
        
        if (str.Length != MAC_CHAR_SIZE) return false;
        
        for (var groups = 0; groups < 6; groups++)
        {
            for (var groupIndex = 0; groupIndex < 2; groupIndex++)
            {
                var index = groups * 2 + groups + groupIndex;
                if (!char.IsLetterOrDigit(str[index])) return false;
            }
        }
        return true;
    }
}
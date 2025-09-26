using System.ComponentModel.DataAnnotations;

namespace WebApplication.API;

public class MinAttribute : RangeAttribute
{
    public MinAttribute(int min) : base(min, int.MaxValue) { }
    
    public MinAttribute(double minimum, double maximum) : base(minimum, maximum) { }

    public MinAttribute(int minimum, int maximum) : base(minimum, maximum) { }

    public MinAttribute(Type type, string minimum, string maximum) : base(type, minimum, maximum) { }
}
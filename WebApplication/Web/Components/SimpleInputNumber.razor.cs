using System.Numerics;

namespace WebApplication.Components.Components;

public partial class SimpleInputNumber<TNumber> : CustomComponentBase
where TNumber : struct, INumberBase<TNumber> { }
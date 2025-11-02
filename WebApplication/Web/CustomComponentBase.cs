using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WebApplication.Components.Components;

namespace WebApplication.Components;

/// <summary>
/// Custom component base 
/// </summary>
public class CustomComponentBase : ComponentBase 
{
    [Inject]
    public required IJSRuntime JsRuntime { get; set; }
    
    [Inject] 
    public required NavigationManager NavigationManager { get; set; }

    /// <summary>
    /// Id if the component.
    /// </summary>
    protected uint Id = IdGenerator.GetId();
}
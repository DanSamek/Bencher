namespace WebApplication.Components.Components;

/// <summary>
/// Arguments when is clicked on previous or next button.
/// </summary>
/// <param name="RequestedPageIndex">Requested index of the page to render</param>
public record PageClickedArgs(int RequestedPageIndex);
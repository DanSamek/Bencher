namespace WebApplication.Components.Components;

/// <summary>
/// Result from the <see cref="TablePaginator.NextPageClicked"/> or <see cref="TablePaginator.PreviousPageClicked"/>.
/// </summary>
/// <param name="Rows">Rows that should be rendered</param>
public record PageClickedResult<TRow>(IReadOnlyList<TRow> Rows);
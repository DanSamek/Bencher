using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data;

/// <summary>
/// Base class for all entities except <see cref="ApplicationUser"/>.
/// </summary>
public class DoId
{
    [Key]
    public required int Id { get; set; }
}
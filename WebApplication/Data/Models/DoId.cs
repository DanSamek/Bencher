using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data.Models;

/// <summary>
/// Base class for all entities except <see cref="ApplicationUser"/>.
/// </summary>
public class DoId
{
    [Key]
    public int Id { get; set; }
}
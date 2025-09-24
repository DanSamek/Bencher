using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data.Models;

/// <summary>
/// Opening book entity.
/// </summary>
public class OpeningBook : DoId
{
    /// <summary>
    /// Name of the opening book.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Opening book data.
    /// </summary>
    [Required]
    public required byte[] Data { get; set; }
    
    /// <summary>
    /// Type of the opening book.
    /// </summary>
    [Required]
    public required OpeningBookType Type { get; set; }
    
    /// <summary>
    /// Book depth.
    /// Aka, how many moves are played from the opening book.
    /// </summary>
    [Required]
    public required int Depth { get; set; }
    
    /// <summary>
    /// Tests that uses this opening book.
    /// </summary>
    [Required]
    public required List<Test> Test { get; set; } = [];
    
    /// <summary>
    /// User, that added opening book.
    /// </summary>
    [Required]
    public ApplicationUser? User { get; set; }
}
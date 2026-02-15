using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data.Models;

public class OpeningBookContent : DoId
{
    private const int MB = 1024 * 1024;
    public const int MEGABYTES_DATA_SIZE = 100; // Should be better to read from .env, but who cares.
    public const int MAX_OPENING_BOOK_SIZE = MEGABYTES_DATA_SIZE * MB;
    
    /// <summary>
    /// Id of the opening book.
    /// </summary>
    public required int OpeningBookId { get; set; }
    
    /// <summary>
    /// Opening book data.
    /// </summary>
    [MaxLength(MAX_OPENING_BOOK_SIZE)]
    public required byte[] Data { get; set; } 
}
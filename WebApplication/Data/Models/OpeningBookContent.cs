namespace WebApplication.Data.Models;

public class OpeningBookContent : DoId
{
    /// <summary>
    /// Id of the opening book.
    /// </summary>
    public required int OpeningBookId { get; set; }
    
    /// <summary>
    /// Opening book data.
    /// </summary>
    public required byte[] Data { get; set; } 
}
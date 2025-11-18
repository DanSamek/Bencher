namespace WebApplication.Data.Models;

public class ErrorContent : DoId
{ 
    /// <summary>
    /// Id of the opening book.
    /// </summary>
    public required int ErrorId { get; set; }
    
    /// <summary>
    /// Opening book data.
    /// </summary>
    public required byte[] Data { get; set; } 
}
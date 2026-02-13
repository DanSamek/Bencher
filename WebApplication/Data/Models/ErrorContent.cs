namespace WebApplication.Data.Models;

public class ErrorContent : DoId
{ 
    /// <summary>
    /// Error id.
    /// </summary>
    public required int ErrorId { get; set; }
    
    /// <summary>
    /// Error data.
    /// </summary>
    public required byte[] Data { get; set; } 
}
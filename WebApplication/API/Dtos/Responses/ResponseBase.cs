namespace WebApplication.API.Dtos.Responses;

public class ResponseBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public ResponseBase() => TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    
    /// <summary>
    /// Timestamp of the response.
    /// </summary>
    public long TimeStamp { get; init; }
}
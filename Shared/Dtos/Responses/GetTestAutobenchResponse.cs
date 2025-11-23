namespace Shared.Dtos.Responses;

public class GetTestAutobenchResponse : ResponseBase
{
    /// <summary>
    /// Technically <see cref="Data.Models.WorkerLog.Id" />.
    /// </summary>
    public int ConnectionId { get; set; }
    
    /// <inheritdoc cref="Data.Models.Engine.GitUrl" />
    public required string GitUrl { get; set; }
    
    /// <inheritdoc cref="Data.Models.TestBranch.Name" />
    public required string TestBranch { get; set; }
    
    /// <inheritdoc cref="Data.Models.Engine.BuildScript" />
    public byte[]? BuildScript { get; set; }
}
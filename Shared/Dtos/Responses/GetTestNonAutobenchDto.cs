namespace Shared.Dtos.Responses;

public class GetTestNonAutobenchResponse : ResponseBase
{
    /// <summary>
    /// Technically <see cref="Data.Models.WorkerLog.Id" />.
    /// </summary>
    public required int ConnectionId { get; set; }
    
    /// <inheritdoc cref="Data.Models.Engine.GitUrl" /> 
    public required string GitUrl { get; set; }
    
    /// <inheritdoc cref="Data.Models.Engine.BuildScript" /> 
    public byte[]? BuildScript { get; set; }
    
    /// <inheritdoc cref="Data.Models.TestBranch.Name" /> 
    public required string TestBranch { get; set; }
    
    /// <inheritdoc cref="Data.Models.TestBranch.Bench" /> 
    public required int TestBranchBench { get; set; }
    
    /// <inheritdoc cref="Data.Models.TestBranch.Name" /> 
    public required string BaseBranch { get; set; }
    
    /// <inheritdoc cref="Data.Models.TestBranch.Bench" /> 
    public required int BaseBranchBench { get; set; }
    
    /// <inheritdoc cref="Data.Models.Test.HashSize" /> 
    public required int HashSize { get; set; }
    
    /// <inheritdoc cref="Data.Models.Test.NumberOfThreads" /> 
    public required int NumberOfThreads { get; set; }
    
    /// <inheritdoc cref="Data.Models.Test.TimeManagement" /> 
    public required string TimeManagement { get; set; }
    
    /// <inheritdoc cref="Data.Models.WorkerLog.TotalNumberOfGames" /> 
    public required int NumberOfGames { get; set; }
    
    /// <inheritdoc cref="Data.Models.OpeningBook" /> 
    public required OpeningBookDto OpeningBook { get; set; }
    
    /// <inheritdoc cref="Data.Models.Test.ExpectedNps" /> 
    public required int ExpectedNps { get; set; }
    
    /// <inheritdoc cref="Data.Models.Test.AdditionalFastChessOptions" /> 
    public string? AdditionalFastchessOptions { get; set; }
}

public record OpeningBookDto(string Name, byte[] Data, OpeningBookType OpeningBookType);
using Microsoft.AspNetCore.Mvc;
using WebApplication.API.Dtos.Responses;
using WebApplication.Data.Models;

namespace WebApplication.API;

public partial class WorkerController
{
    private OkObjectResult HandleAutobenchResponse(WorkerLog workerLog, Test test)
    {
        var testBranch = _testBranchStore.GetById(test.TestBranchId); // TestBranch shouldn't be null - test is already created.
        var result = new GetTestAutobenchResponse
        {
            ConnectionId = workerLog.Id,
            GitUrl =  test.Engine.GitUrl,
            TestBranch = testBranch!.Name,
            BuildScript = test.Engine.BuildScript
        };
        return Ok(result);
    }

    private OkObjectResult HandleNormalTestResponse(WorkerLog workerLog, Test test)
    {
        var testBranch = _testBranchStore.GetById(test.TestBranchId);
        var baseBranch = _testBranchStore.GetById(test.BaseBranchId);
        
        var result = new GetTestNonAutobenchResponse
        {
            ConnectionId = workerLog.Id,
            GitUrl = test.Engine.GitUrl,
            TestBranch = testBranch!.Name,
            TestBranchBench = testBranch.Bench,
            BaseBranch = baseBranch!.Name,
            BaseBranchBench = baseBranch.Bench,
            HashSize = test.HashSize,
            NumberOfThreads = test.NumberOfThreads,
            TimeManagement = test.TimeManagement,
            NumberOfGames = workerLog.TotalNumberOfGames,
            OpeningBook = new OpeningBookDto(test.OpeningBook.Name, test.OpeningBook.Data, test.OpeningBook.Depth)
        };
        
        return Ok(result);
    }

}
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.Responses;
using WebApplication.Data.Models;

namespace WebApplication.API;

public partial class WorkerController
{
    private OkObjectResult HandleAutobenchResponse(WorkerLog workerLog, Test test)
    {
        var testBranch = _workerControllerService.GetTestBranch(test.TestBranchId);
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
        var testBranch = _workerControllerService.GetTestBranch(test.TestBranchId);
        var baseBranch = _workerControllerService.GetTestBranch(test.BaseBranchId);

        var content = _workerControllerService.GetContentForOpeningBook(test.OpeningBook.Id);
        var result = new GetTestNonAutobenchResponse
        {
            AdditionalFastchessOptions = test.AdditionalFastchessOptions,
            ExpectedNps = test.ExpectedNps,
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
            OpeningBook = new OpeningBookDto(test.OpeningBook.Name, content, test.OpeningBook.Type),
            BuildScript = test.Engine.BuildScript
        };
        
        return Ok(result);
    }

}
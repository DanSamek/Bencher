using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Dtos.Requests;
using Shared.Dtos.Responses;
using WebApplication.Data.Models;
using WebApplication.SPRT;
using WebApplication.Stores;

namespace WebApplication.API;

/// <summary>
/// Controller for worker -- server communication.
/// </summary>
[ApiController]
[Route(Constants.WORKER_API_PREFIX)]
public partial class WorkerController : ControllerBase  
{   
    private readonly UserStore _userStore;
    private readonly WorkerLogStore _workerLogStore;
    private readonly PentaStore _pentaStore;
    private readonly TestStore _testStore;
    private readonly TestBranchStore _testBranchStore;
    private readonly AutobenchStateStore _autobenchStateStore;
    private readonly OpeningBookStore _openingBookStore;
    private readonly WorkerErrorStore _workerErrorStore;
    
    private static readonly SemaphoreSlim _resultsSemaphore = new SemaphoreSlim(1, 1);
    private static readonly SemaphoreSlim _getTestSemaphore = new SemaphoreSlim(1, 1);
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public WorkerController(UserStore userStore, WorkerLogStore workerLogStore, PentaStore pentaStore, TestStore testStore, TestBranchStore testBranchStore, AutobenchStateStore autobenchStateStore, OpeningBookStore openingBookStore, WorkerErrorStore workerErrorStore)
    {
        _userStore = userStore;
        _workerLogStore = workerLogStore;
        _pentaStore = pentaStore;
        _testStore = testStore;
        _testBranchStore = testBranchStore;
        _autobenchStateStore = autobenchStateStore;
        _openingBookStore = openingBookStore;
        _workerErrorStore = workerErrorStore;
    }
    
    /// <summary>
    /// Action for workers - when error occurs before running any tests.
    /// For example missing dependencies [git,..]
    /// </summary>
    /// <returns></returns>
    [HttpPost("worker-error")]
    public IActionResult WorkerError([FromBody] WorkerErrorDto workerErrorDto)
    {
        _workerErrorStore.AddError(workerErrorDto.Log);
        return Ok(new ResponseBase());
    }
    
    /// <summary>
    /// Action for workers - when error occurs when running test.
    /// </summary>
    [HttpPost("test-error")]
    public async Task<IActionResult> TestError([FromBody] TestErrorDto testErrorDto)
    {
        var workerLog = _workerLogStore.GetByConnectionId(testErrorDto.ConnectionId);
        if (workerLog is null || testErrorDto.Log.Length > Constants.MAX_LOG_FILE_SIZE) return NotFound(new ResponseBase());
        
        workerLog.State = WorkerLogState.Finished;
        _workerLogStore.AddError(workerLog, testErrorDto.Log);
        
        await StopTest(workerLog.Test.Id);
        return Ok(new ResponseBase());
    }
    
    /// <summary>
    /// Action for workers - sending classical SPRT results.
    /// </summary>
    [HttpPost("results")]
    public async Task<IActionResult> Results([FromBody] ResultsDto resultsDto)
    {
        // It's not ideal to use semaphore for the entire controller action (maybe TODO)
        await _resultsSemaphore.WaitAsync();
        try
        {
            var workerLog = _workerLogStore.GetByConnectionId(resultsDto.ConnectionId);
            if (workerLog is null) return NotFound(new ResponseBase());

            if (workerLog.State != WorkerLogState.Active) return NotFound(new ResponseBase()); // TODO update docs.
            if (workerLog.Test.State != TestState.Running) return Ok(new ResultsResponseDto(false));

            // Test can be eventually deleted !
            var test = _testStore.GetById(workerLog.Test.Id);
            if (test is null) return NotFound(new ResponseBase()); // TODO update docs.
            
            var toIncrement = (resultsDto.Ll + resultsDto.Ld + resultsDto.Dd + resultsDto.Wl + resultsDto.Wd + resultsDto.Ww) * 2;
            if (workerLog.NumberOfGames + toIncrement > workerLog.TotalNumberOfGames) return NotFound(new ResponseBase()); // TODO update docs.

            await _pentaStore.UpdatePenta(workerLog.Test.Id, resultsDto.Ll, resultsDto.Ld, resultsDto.Dd, resultsDto.Wl,
                resultsDto.Wd, resultsDto.Ww);
            workerLog.NumberOfGames += toIncrement;
            if (workerLog.NumberOfGames == workerLog.TotalNumberOfGames)
            {
                workerLog.State = WorkerLogState.Finished;
            }

            _workerLogStore.Update(workerLog);

            var running = await _testStore.SetPausedIfNoActiveWorkers(workerLog.Test.Id);
            
            // SPRT part.
            var statistics = Sprt.GetStatistics(test);
            if (statistics.Result != Sprt.SprtResult.Unknown)
            {
                await _workerLogStore.StopAllWorkers(workerLog.Test.Id);
                await _testStore.SetFinishedState(test.Id);
            }

            return Ok(new ResultsResponseDto(running && statistics.Result == Sprt.SprtResult.Unknown));
        }
        finally
        {
            _resultsSemaphore.Release();
        }
    }
    
    /// <summary>
    /// Action for workers - sending autobench result.
    /// </summary>
    [HttpPost("autobench")]
    public async Task<IActionResult> Autobench([FromBody] AutobenchDto autobenchDto)
    {
        var workerLog = _workerLogStore.GetByConnectionId(autobenchDto.ConnectionId);
        if (workerLog is null || workerLog.State != WorkerLogState.Active) return NotFound(new ResponseBase());
        
        var autobenchState = _autobenchStateStore.GetAutobenchStateByTestId(workerLog.Test.Id);
        if (autobenchState is null) return NotFound(new ResponseBase());

        var result = autobenchState.UpdateConfidence(autobenchDto.Autobench);
        if (!result)
        {
            await StopTest(workerLog.Test.Id);
        }
        
        workerLog.State = WorkerLogState.Finished;
        _workerLogStore.SaveChanges();
        _autobenchStateStore.SaveChanges();
        
        if (result)
        {
            await _testStore.SetPausedIfNoActiveWorkers(workerLog.Test.Id);
        }
        
        // If test is resolved, set this as a bench of the test branch.
        if (autobenchState.Resolved)
        {
            await _testBranchStore.SetTestBranchBench(workerLog.Test.Id, autobenchState.Bench);
        }
        
        return Ok(new ResponseBase());
    }
    
    /// <summary>
    /// Request of the worker for a test.
    /// </summary>
    [HttpPost("get-test")]
    public async Task<IActionResult> GetTest([FromBody] GetTestDto getTestDto)
    {
        // NOTE: This is not ideal, but we ensure for thread splits at workers to work :(( 
        await _getTestSemaphore.WaitAsync();

        Test? test;
        try
        {
            test = _testStore.GetNextTestForWorker(getTestDto.Autobench, getTestDto.NumberOfThreads);
            if (test is null) return NotFound(new ResponseBase());

            await _testStore.SetRunningState(test.Id);
        }
        finally
        {
            _getTestSemaphore.Release();
        }
        
        var userToken = HttpContext.GetUserToken();
        var user = _userStore.GetUserByAccessToken(userToken);
        
        _workerLogStore.Attach(user!);
        _workerLogStore.Attach(test);

        var now = DateTime.UtcNow;
        var wl = new WorkerLog
        {
            Name = getTestDto.Name.Length <= WorkerLog.MAX_NAME_LENGTH ? getTestDto.Name : getTestDto.Name[..(WorkerLog.MAX_NAME_LENGTH - 1)],
            InitialTestState = getTestDto.Autobench ? InitialTestState.Autobenched : InitialTestState.Normal,
            State = WorkerLogState.Active,
            ConnectTime = now,
            LastConnectTime = now,
            NumberOfGames = 0,
            TotalNumberOfGames = getTestDto.NumberOfThreads * Constants.GAME_THREAD_COUNT_MULTIPLIER, 
            NumberOfThreads = getTestDto.NumberOfThreads,
            Mac = getTestDto.Mac,
            User = user!, // User exists - middleware validated that.
            Test = test
        };
        
        _workerLogStore.Add(wl);
        _workerLogStore.SaveChanges();
        
        return getTestDto.Autobench ? HandleAutobenchResponse(wl, test) : HandleNormalTestResponse(wl, test);
    }
    
    /// <summary>
    /// Notification to the server, that test is still running on the worker
    /// and worker is still running. 
    /// </summary>
    [HttpPost("running-test")]
    public IActionResult RunningTest([FromBody] RunningTestDto runningTestDto)
    {
        var workerLog = _workerLogStore.GetByConnectionId(runningTestDto.ConnectionId);
        if (workerLog is null || workerLog.State != WorkerLogState.Active) return NotFound(new ResponseBase());
        _testStore.SetRunningState(workerLog.Test);
        
        workerLog.LastConnectTime = DateTime.UtcNow;
        _workerLogStore.Update(workerLog);
        _workerLogStore.SaveChanges();
        
        var running = workerLog.Test.State.Running();
        var result = new RunningTestResponseDto(running);
        return Ok(result);
    }
    
    /// <summary>
    /// Validates users access token - used as a simple login.
    /// </summary>
    [HttpPost("validate")]
    public IActionResult Validate()
    {
        var userToken = HttpContext.GetUserToken();
        
        var user = _userStore.GetUserByAccessToken(userToken);
        if (user is null) return Unauthorized(new ResponseBase());
        
        var result = new ValidateResponseDto(user.UserName!);
        return Ok(result);
    }

    /// <summary>
    /// Returns number of paused tests (with the maximum possible priority).
    /// </summary>
    [HttpGet("total-paused-tests")]
    public IActionResult TotalPausedTestsWithMaxPriority()
    {
        var result = new TotalPausedTestsDto
        {
            Count = _testStore.TotalPausedTestsWithMaxPriority()
        };
        return Ok(result);
    }
    
    /// <summary>
    /// Returns maximum required threads for the test (with the highest priority).
    /// </summary>
    [HttpGet("max-threads-for-test")]
    public IActionResult MaxThreadsForTestWithMaxPriority()
    {
        var result = new MaxThreadsForTestDto
        {
            MaximumThreads = _testStore.MaxThreadsForTestWithMaxPriority()
        };
        return Ok(result);
    }
    
    private async Task StopTest(int testId)
    {
        await _testStore.StopTest(testId);
        await _workerLogStore.StopAllWorkers(testId);
    }
}

file static class HttpContextExtensions 
{
    public static string GetUserToken(this HttpContext context)
    => context.Request.Headers[Constants.WORKER_REQUEST_HEADER].ToString(); // Middleware validated, that user token is valid.
}

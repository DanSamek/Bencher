using Microsoft.AspNetCore.Mvc;
using WebApplication.API.Dtos.Requests;
using WebApplication.API.Dtos.Responses;
using WebApplication.Data.Models;
using WebApplication.Stores;

namespace WebApplication.API;

/// <summary>
/// Controller for worker -- server communication.
/// </summary>
[ApiController]
[Route(Shared.WORKER_API_PREFIX)]
public partial class WorkerController : ControllerBase
{
    private readonly UserStore _userStore;
    private readonly WorkerLogStore _workerLogStore;
    private readonly PentaStore _pentaStore;
    private readonly TestStore _testStore;
    private readonly TestBranchStore _testBranchStore;
    private readonly AutobenchStateStore _autobenchStateStore;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public WorkerController(UserStore userStore, WorkerLogStore workerLogStore, PentaStore pentaStore, TestStore testStore, TestBranchStore testBranchStore, AutobenchStateStore autobenchStateStore)
    {
        _userStore = userStore;
        _workerLogStore = workerLogStore;
        _pentaStore = pentaStore;
        _testStore = testStore;
        _testBranchStore = testBranchStore;
        _autobenchStateStore = autobenchStateStore;
    }
        
    /// <summary>
    /// Action for workers error of the test.
    /// </summary>
    [HttpPost("error")]
    public IActionResult Error([FromBody] ErrorDto errorDto)
    {
        var workerLog = _workerLogStore.GetByConnectionId(errorDto.ConnectionId);
        if (workerLog is null || errorDto.Log.Length > Shared.MAX_LOG_FILE_SIZE) return NotFound();

        // TODO move to the stores
        using var memoryStream = new MemoryStream();
        errorDto.Log.CopyTo(memoryStream);
        
        var error = new Error
        {
            Time = DateTime.Now,
            Log = memoryStream.ToArray(),
            Test = workerLog.Test,
            WorkerLog = workerLog
        };
        
        workerLog.Errors.Add(error);
        _workerLogStore.Save(workerLog);
        
        _testStore.SetState(workerLog.Test, TestState.Stopped);
        
        return Ok(new ResponseBase());
    }
    
    /// <summary>
    /// Action for workers classical SPRT results.
    /// </summary>
    [HttpPost("results")]
    public async Task<IActionResult> Results([FromBody] ResultsDto resultsDto)
    {
        var workerLog = _workerLogStore.GetByConnectionId(resultsDto.ConnectionId);
        if (workerLog is null) return NotFound();
        
        if (!workerLog.Test.State.Running()) return Ok(new ResultsResponseDto(false));

        await _pentaStore.UpdatePenta(workerLog.Test.Id, resultsDto.Ll, resultsDto.Ld, resultsDto.Dd, resultsDto.Wl, resultsDto.Wd, resultsDto.Ww);
        return Ok(new ResultsResponseDto(true));
    }
    
    /// <summary>
    /// Action for workers autobench result.
    /// </summary>
    [HttpPost("autobench")]
    public async Task<IActionResult> Autobench([FromBody] AutobenchDto autobenchDto)
    {
        var workerLog = _workerLogStore.GetByConnectionId(autobenchDto.ConnectionId);
        if (workerLog is null) return NotFound(new ResponseBase());
        
        var autobenchState = _autobenchStateStore.GetAutobenchStateByTestId(workerLog.Test.Id);
        if (autobenchState is null) return NotFound(new ResponseBase());

        var result = autobenchState.UpdateConfidence(autobenchDto.Autobench);
        if (!result) await _testStore.StopTest(workerLog.Test.Id);
        
        return Ok(new ResponseBase());
    }
    
    /// <summary>
    /// Request of the worker for a test.
    /// </summary>
    [HttpPost("get-test")]
    public IActionResult GetTest([FromBody] GetTestDto getTestDto)
    {
        var test = _testStore.GetNextTestForWorker(getTestDto.Autobench, getTestDto.NumberOfThreads);
        if (test is null) return NotFound(new ResponseBase());
        
        var userToken = HttpContext.GetUserToken();
        var user = _userStore.GetUserByAccessToken(userToken);

        // TODO move to the store
        var wl = new WorkerLog
        {
            ConnectTime = DateTime.Now,
            NumberOfGames = 0,
            TotalNumberOfGames = getTestDto.NumberOfThreads * Shared.GAME_THREAD_COUNT_MULTIPLIER, 
            NumberOfThreads = getTestDto.NumberOfThreads,
            Mac = getTestDto.Mac,
            User = user!, // User exists - middleware validated that.
            Test = test
        };
        
        _workerLogStore.Create(wl);
        test.WorkerLogs.Add(wl);
        _testStore.Update(test);
        
        // Should be okay -- when disposing stores we call Context.SaveChanges()
        //_workerLogStore.SaveChanges();
        //_testStore.SaveChanges();
        
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
        if (workerLog is null) return NotFound();

        // TODO move to the store.
        // First information from the worker.
        // Set running || autobenched.
        if (workerLog.LastConnectTime is null && workerLog.Test.State == TestState.Paused)
        {
            var state = workerLog.Test.AutobenchState is null || workerLog.Test.AutobenchState!.Resolved
                        ? TestState.Running : TestState.Autobenched;
            
            _testStore.SetState(workerLog.Test, state);
        }
        
        workerLog.LastConnectTime = DateTime.Now;
        _workerLogStore.Save(workerLog);
        
        var running = workerLog.Test.State.Running();
        var result = new RunningTestResponseDto(running);
        return Ok(result);
    }
    
    /// <summary>
    /// Validates users access token - used as simple login.
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
}

file static class HttpContextExtensions
{
    public static string GetUserToken(this HttpContext context)
    => context.Request.Headers[Shared.WORKER_REQUEST_HEADER].ToString(); // Middleware validated, that user token is valid.
}

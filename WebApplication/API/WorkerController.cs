using Microsoft.AspNetCore.Mvc;
using WebApplication.API.Dtos.Requests;
using WebApplication.API.Dtos.Responses;
using WebApplication.Data.Models;

namespace WebApplication.API;

/// <summary>
/// Controller for worker -- server communication.
/// </summary>
[ApiController]
[Route(Shared.WORKER_API_PREFIX)]
public class WorkerController : ControllerBase
{
    private readonly Stores.UserStore _userStore;
    private readonly Stores.WorkerLogStore _workerLogStore;
    private readonly Stores.PentaStore _pentaStore;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public WorkerController(Stores.UserStore userStore, Stores.WorkerLogStore workerLogStore, Stores.PentaStore pentaStore)
    {
        _userStore = userStore;
        _workerLogStore = workerLogStore;
        _pentaStore = pentaStore;
    }
        
    /// <summary>
    /// Action for workers error of the test.
    /// </summary>
    [HttpPost("error")]
    public IActionResult Error([FromBody] ErrorDto errorDto)
    {
        var workerLog = _workerLogStore.GetByConnectionId(errorDto.ConnectionId);
        if (workerLog is null) return NotFound();

        if (errorDto.Log.Length > Shared.MAX_LOG_FILE_SIZE) return NotFound(); // TODO maybe in Program.cs configure mox. file upload
        using var memoryStream = new MemoryStream();
        errorDto.Log.CopyTo(memoryStream);
        
        var error = new Error
        {
            Time = DateTime.Now,
            Log = memoryStream.ToArray(), 
            Test = workerLog.Test,
            WorkerLog = workerLog,
        };
        
        workerLog.Errors.Add(error);
        _workerLogStore.Save(workerLog);
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
    public IActionResult Autobench([FromBody] AutobenchDto autobenchDto)
    {
        return Ok();
    }
    
    /// <summary>
    /// Request of the worker for a test.
    /// </summary>
    [HttpPost("get-test")]
    public IActionResult GetTest([FromBody] GetTestDto getTestDto)
    {
        return Ok();
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
        if (!Request.Headers.TryGetValue(Shared.WORKER_REQUEST_HEADER, out var value)) return Unauthorized();
        
        var user = _userStore.GetUserByAccessToken(value.ToString());
        if (user is null) return Unauthorized(new ResponseBase());
        
        var result = new ValidateResponseDto(user.UserName!);
        return Ok(result);
    }
}
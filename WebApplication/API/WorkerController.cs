using Microsoft.AspNetCore.Mvc;
using WebApplication.API.Dtos.Requests;
using WebApplication.API.Dtos.Responses;
using WebApplication.Data.Models;

namespace WebApplication.API;

/// <summary>
/// Controller for worker -- server communication.
/// </summary>
[ApiController]
[Route("worker-api/")]
public class WorkerController : Controller
{
    private readonly Stores.UserStore _userStore;
    private readonly Stores.WorkerLogStore _workerLogStore;
    private readonly Stores.TestStore _testStore;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public WorkerController(Stores.UserStore userStore, Stores.WorkerLogStore workerLogStore, Stores.TestStore testStore)
    {
        _userStore = userStore;
        _workerLogStore = workerLogStore;
        _testStore = testStore;
    }
        
    /// <summary>
    /// Action for workers error of the test.
    /// </summary>
    [HttpPost("error")]
    public IActionResult Error([FromBody] ErrorDto errorDto)
    {
        return Ok(new ResponseBase());
    }
    
    /// <summary>
    /// Action for workers classical SPRT results.
    /// </summary>
    [HttpPost("results")]
    public IActionResult Results([FromBody] ResultsDto resultsDto)
    {
        var workerLog = _workerLogStore.GetByConnectionId(resultsDto.ConnectionId);
        if (workerLog is null) return NotFound();
        // Technically is also called /running, but KISS.
        var test = _testStore.GetById(workerLog.Test.Id);
        if (test is null) return NotFound(); // <- here test can be deleted.
        
        if (!test.State.Running()) return Ok(new ResultsResponseDto(false));
        
        // TODO make sure, that Penta.Ll += xx is okay multithreaded.
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
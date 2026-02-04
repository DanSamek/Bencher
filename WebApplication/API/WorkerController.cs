using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Dtos.Requests;
using Shared.Dtos.Responses;

namespace WebApplication.API;

/// <summary>
/// Controller for worker -- server communication.
/// </summary>
[ApiController]
[Route(Constants.WORKER_API_PREFIX)]
public partial class WorkerController : ControllerBase  
{   
    private readonly IWorkerControllerService _workerControllerService;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public WorkerController(IWorkerControllerService workerControllerService)
    {
        _workerControllerService = workerControllerService;
    }
    
    /// <summary>
    /// Action for workers - when error occurs before running any tests.
    /// For example missing dependencies [git,..]
    /// </summary>
    /// <returns></returns>
    [HttpPost("worker-error")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBase))]
    public IActionResult WorkerError([FromBody] WorkerErrorDto workerErrorDto)
    {
        _workerControllerService.AddWorkerError(workerErrorDto);
        return Ok(new ResponseBase());
    }
    
    /// <summary>
    /// Action for workers - when error occurs when running test.
    /// </summary>
    [HttpPost("test-error")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBase))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseBase>> TestError([FromBody] TestErrorDto testErrorDto)
    {
        await _workerControllerService.SaveTestError(testErrorDto);
        return Ok(new ResponseBase());
    }
    
    /// <summary>
    /// Action for workers - sending classical SPRT results.
    /// </summary>
    [HttpPost("results")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultsResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultsResponseDto>> Results([FromBody] ResultsDto resultsDto)
    {
        var isTestStillRunning = await _workerControllerService.UpdateSPRTResults(resultsDto);
        return Ok(new ResultsResponseDto(isTestStillRunning));
    }
    
    /// <summary>
    /// Action for workers - sending autobench result.
    /// </summary>
    [HttpPost("autobench")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseBase))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseBase>> Autobench([FromBody] AutobenchDto autobenchDto)
    {
        await _workerControllerService.UpdateAutobenchState(autobenchDto);
        return Ok(new ResponseBase());
    }
    
    /// <summary>
    /// Request of the worker for a test.
    /// </summary>
    [HttpPost("get-test")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetTestAutobenchResponse))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetTestNonAutobenchResponse))]
    public async Task<IActionResult> GetTest([FromBody] GetTestDto getTestDto)
    {
        var (wl, test) = await _workerControllerService.CreateJobForWorker(getTestDto, HttpContext.GetAccessToken());
        return getTestDto.Autobench ? HandleAutobenchResponse(wl, test) : HandleNormalTestResponse(wl, test);
    }
    
    /// <summary>
    /// Notification to the server, that test is still running on the worker
    /// and worker is still running. 
    /// </summary>
    [HttpPost("running-test")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RunningTestResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<RunningTestResponseDto> RunningTest([FromBody] RunningTestDto runningTestDto)
    {
        var stillRunning = _workerControllerService.HandleRunningTestFromWorker(runningTestDto.ConnectionId);
        var result = new RunningTestResponseDto(stillRunning);
        return Ok(result);
    }
    
    /// <summary>
    /// Validates users access token - used as a simple login.
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ValidateResponseDto))]
    public ActionResult<ValidateResponseDto> Validate()
    {
        var accessToken = HttpContext.GetAccessToken();
        var username = _workerControllerService.GetUsernameByAccessToken(accessToken);
        var result = new ValidateResponseDto(username);
        return Ok(result);
    }

    /// <summary>
    /// Returns number of paused tests (with the maximum possible priority).
    /// </summary>
    [HttpGet("total-paused-tests")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TotalPausedTestsDto))]
    public ActionResult<TotalPausedTestsDto> TotalPausedTestsWithMaxPriority()
    {
        var totalPausedTests = _workerControllerService.GetTotalPausedTestsWithMaxPriority();
        var result = new TotalPausedTestsDto
        {
            Count = totalPausedTests
        };
        return Ok(result);
    }
    
    /// <summary>
    /// Returns maximum required threads for the test (with the highest priority).
    /// </summary>
    [HttpGet("max-threads-for-test")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MaxThreadsForTestDto))]
    public ActionResult<MaxThreadsForTestDto> MaxThreadsForTestWithMaxPriority()
    {
        var maxThreads = _workerControllerService.GetMaxThreadsForTestWithMaxPriority();
        var result = new MaxThreadsForTestDto
        {
            MaximumThreads = maxThreads
        };
        return Ok(result);
    }
}

file static class HttpContextExtensions 
{
    public static string GetAccessToken(this HttpContext context)
        => context.Request.Headers[Constants.WORKER_REQUEST_HEADER].ToString(); // Middleware validated, that user token is valid.
}

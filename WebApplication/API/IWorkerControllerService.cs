using Shared.Dtos.Requests;
using WebApplication.Data.Models;

namespace WebApplication.API;

public interface IWorkerControllerService
{
    /// <summary>
    /// Sets workerlog state to running and sets test state to running.
    /// </summary>
    /// <param name="connectionId">Worker's connection id</param>
    /// <exception cref="NotFoundException">Is thrown if workerlog is null or is not active.</exception>
    public bool HandleRunningTestFromWorker(int connectionId);

    /// <summary>
    /// Saves test error log from the worker
    /// </summary>
    /// <exception cref="NotFoundException">Is thrown if workerlog is null or log is too big.</exception>
    public Task SaveTestError(TestErrorDto testErrorDto);

    /// <summary>
    /// Updates SPRT results for the test.
    /// </summary>
    /// <returns>If test is still running</returns>
    /// <exception cref="NotFoundException"></exception>
    public Task<bool> UpdateSPRTResults(ResultsDto resultsDto);

    /// <summary>
    /// Updates autobench state of the test.
    /// </summary>
    /// <param name="autobenchDto"></param>
    /// <exception cref="NotFoundException"></exception>
    public Task UpdateAutobenchState(AutobenchDto autobenchDto);

    /// <summary>
    /// Returns a test for a worker.
    /// </summary>
    /// <exception cref="NotFoundException">Is thrown if there is no test for a worker.</exception>
    public Task<(WorkerLog, Test)> CreateJobForWorker(GetTestDto getTestDto, string userToken);

    /// <summary>
    /// Returns username for the access token of the user. 
    /// </summary>
    public string GetUsernameByAccessToken(string accessToken);

    /// <summary>
    /// Saves worker error. 
    /// </summary>
    /// <param name="workerErrorDto"></param>
    public void AddWorkerError(WorkerErrorDto workerErrorDto);
    
    /// <summary>
    /// Returns number of paused tests with maximum priority.
    /// </summary>
    public int GetTotalPausedTestsWithMaxPriority();
    
    /// <summary>
    /// Returns maximum required threads for the test (with the maximum priority).
    /// </summary>
    public int GetMaxThreadsForTestWithMaxPriority();

    /// <summary>
    /// Returns testbranch by given id.
    /// </summary>
    public TestBranch? GetTestBranch(int testBranchId);

    /// <summary>
    /// Loads opening book content from the database by given id.
    /// </summary>
    /// <returns></returns>
    byte[] GetContentForOpeningBook(int openingBookId);
}
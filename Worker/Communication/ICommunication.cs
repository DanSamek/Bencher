using Shared.Dtos.Requests;
using Shared.Dtos.Responses;

namespace Worker;

public interface ICommunication
{
    /// <summary>
    /// If error occured when request was send.
    /// </summary>
    public bool Error();

    /// <summary>
    /// Returns error message.
    /// </summary>
    public string GetErrorMessage();

    /// <summary>
    /// Implementation of the /get-test for autobench
    /// </summary>
    /// <returns>returns <see cref="Shared.Dtos.Responses.GetTestNonAutobenchResponse"/> if any test is in the queue, else null</returns>
    public GetTestNonAutobenchResponse? TryGetTest();

    /// <summary>
    /// Implementation of the /get-test for standard test.
    /// </summary>
    /// <returns>returns <see cref="Shared.Dtos.Responses.GetTestAutobenchResponse"/> if any test is in the queue, else null</returns>
    public GetTestAutobenchResponse? TryGetAutobenchTest();
    
    /// <summary>
    /// Notifies a server, that this workload is still running at the worker.
    /// </summary>
    /// <param name="connectionId">Connection id, that was obtained from <see cref="TryGetTest"/> <see cref="TryGetAutobenchTest"/></param>
    public RunningTestResponseDto? RunningTest(int connectionId);
    
    /// <summary>
    /// Sends autobench result to the server. 
    /// </summary>
    public void SendAutobenchResult(int autobench, int connectionId);

    /// <summary>
    /// Sends pentanomial results to the server.
    /// </summary>
    public ResultsResponseDto? Results(ResultsDto dto);

    /// <summary>
    /// Sends worker error to the server. 
    /// </summary>
    /// <param name="errorTrace">Trace with the error</param>
    public void WorkerError(ErrorTrace errorTrace);

    /// <summary>
    /// Sends test error to the server. 
    /// </summary>
    /// <param name="errorTrace">Trace with the error</param>
    /// <param name="connectionId">Connection id, that was obtained from <see cref="TryGetTest"/> <see cref="TryGetAutobenchTest"/></param> 
    public void TestError(ErrorTrace errorTrace, int connectionId);

    /// <summary>
    /// Returns maximum possible threads for the test. 
    /// </summary>
    public int MaxThreadsForTest();
    
    /// <summary>
    /// Returns number of paused tests.
    /// </summary>
    public int TotalPausedTests();
}
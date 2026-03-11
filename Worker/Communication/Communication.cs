using System.Net.NetworkInformation;
using System.Text;
using Newtonsoft.Json;
using Shared;
using Shared.Dtos.Requests;
using Shared.Dtos.Responses;
using Worker.UI;

namespace Worker.Communication;

/// <summary>
/// Implementation of the communication with the server.
/// </summary>
public class Communication : ICommunication
{
    private readonly RunnerOptions _options;
    private readonly string _workerMac;
    private readonly HttpClient _client;
    private readonly ErrorTrace _errorTrace;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    /// <param name="options"></param>
    public Communication(RunnerOptions options, HttpClient client)
    {
        _options = options;
        _workerMac = GetWorkerMac();
        _client = client;
        _errorTrace = new ErrorTrace();
    }
    
    /// <inheritdoc /> 
    public bool Error() => _errorTrace.Error();

    /// <inheritdoc /> 
    public string GetErrorMessage() => _errorTrace.ToString();
    
    /// <inheritdoc /> 
    public GetTestNonAutobenchResponse? TryGetTest()
        => GetTest<GetTestNonAutobenchResponse>(false);
    
    /// <inheritdoc /> 
    public GetTestAutobenchResponse? TryGetAutobenchTest()
        => GetTest<GetTestAutobenchResponse>(true);
    
    /// <inheritdoc /> 
    public RunningTestResponseDto? RunningTest(int connectionId)
    {
        var runningDto = new RunningTestDto
        {
            ConnectionId = connectionId
        };
        try
        {
            var requestMessage =
                new HttpRequestMessage(HttpMethod.Post, $"/{Constants.WORKER_API_PREFIX}/running-test");
            var responseContent = SendAndDeserialize<RunningTestResponseDto, RunningTestDto>(requestMessage, runningDto);
            return responseContent;
        }
        catch (Exception ex)
        {
            _errorTrace.AddError(ex.Message);
        }
        return null;
    }

    /// <inheritdoc /> 
    public void SendAutobenchResult(int autobench, int connectionId)
    {
        var runningDto = new AutobenchDto
        {
            ConnectionId = connectionId,
            Autobench = autobench
        };

        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/{Constants.WORKER_API_PREFIX}/autobench");
            SendAndDeserialize<ResponseBase, AutobenchDto>(requestMessage, runningDto);
        }
        catch (Exception ex)
        {
            _errorTrace.AddError(ex.Message);
        }
    }

    /// <inheritdoc /> 
    public void WorkerError(ErrorTrace errorTrace)
    {
        var workerErrorDto = new WorkerErrorDto
        {
            Log = errorTrace.GetBytes()
        };
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/{Constants.WORKER_API_PREFIX}/test-error");
            SendAndDeserialize<ResponseBase, WorkerErrorDto>(requestMessage, workerErrorDto);
        }
        catch (Exception ex)
        {
            _errorTrace.AddError(ex.Message);
        }
    }
    
    /// <inheritdoc />
    public ResultsResponseDto? Results(ResultsDto dto)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/{Constants.WORKER_API_PREFIX}/results");
            var result = SendAndDeserialize<ResultsResponseDto, ResultsDto>(requestMessage, dto);
            return result;
        }
        catch (Exception ex)
        {
            _errorTrace.AddError(ex.Message);
            return null;
        }
    }

    /// <inheritdoc /> 
    public void TestError(ErrorTrace errorTrace, int connectionId)
    {
        var testErrorDto = new TestErrorDto
        {
            Log = errorTrace.GetBytes(),
            ConnectionId = connectionId
        };
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/{Constants.WORKER_API_PREFIX}/test-error");
            SendAndDeserialize<ResponseBase, TestErrorDto>(requestMessage, testErrorDto);
        }
        catch (Exception ex)
        {
            _errorTrace.AddError(ex.Message);
        }
    }
    
    /// <inheritdoc /> 
    public int MaxThreadsForTest()
    {
        const int DEFAULT_THREADS = 1;
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/{Constants.WORKER_API_PREFIX}/max-threads-for-test");
            var response = _client.Send(requestMessage);
            if (!response.IsSuccessStatusCode) return DEFAULT_THREADS;
            
            var result = Helper.Deserialize<MaxThreadsForTestDto>(response);
            return result?.MaximumThreads ?? DEFAULT_THREADS;
        }
        catch (Exception ex)
        {
            _errorTrace.AddError(ex.Message);
        }
        return DEFAULT_THREADS;
    }
    
    /// <inheritdoc /> 
    public int TotalPausedTests()
    {
        const int DEFAULT_PAUSED_TESTS = 0;
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/{Constants.WORKER_API_PREFIX}/total-paused-tests");
            var response = _client.Send(requestMessage);
            if (!response.IsSuccessStatusCode) return DEFAULT_PAUSED_TESTS;
            var result = Helper.Deserialize<TotalPausedTestsDto>(response);
            return result?.Count ?? DEFAULT_PAUSED_TESTS;
        }
        catch (Exception ex)
        {
            _errorTrace.AddError(ex.Message);
        }
        return DEFAULT_PAUSED_TESTS;
    }

    private T? GetTest<T>(bool autobench)
        where T : class
    {
        try
        {
            var uri = CreateGetTestUri(autobench);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/{Constants.WORKER_API_PREFIX}/test?{uri}");
            var result = _client.Send(requestMessage); 
            if (!result.IsSuccessStatusCode) return null;
            var responseContent = Helper.Deserialize<T>(result);
            return responseContent;
        }
        catch (Exception ex)
        {
            _errorTrace.AddError(ex.Message);
        }
        return null;
    }
    
    private string GetWorkerMac()
    {
        var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
            .First(ni =>
                ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);
        
        var mac = networkInterface.GetPhysicalAddress().ToString();
        var sb = new StringBuilder();
        
        var append = false;
        for (var i = 0; i < mac.Length; i++, append = !append)
        {
            sb.Append(mac[i]);
            if (append && (i + 1) != mac.Length)
            {
                sb.Append(':');
            }
        }

        var result = sb.ToString();
        return result;
    }

    private string CreateGetTestUri(bool autobench)
    {
        var uri = $"Autobench={autobench}&Mac={_workerMac}&Name={Environment.MachineName}&NumberOfThreads={_options.NumberOfThreads}";
        /*
        var dto = new GetTestDto
        {
            Autobench = autobench,
            Mac = _workerMac,
            Name = Environment.MachineName,
            NumberOfThreads = _options.NumberOfThreads
        };
        */
        return uri;
    }
    
    private TOut? SendAndDeserialize<TOut, TIn>(HttpRequestMessage requestMessage, TIn dto) 
        where TOut : class
    {
        var serializedContent = JsonConvert.SerializeObject(dto);
        requestMessage.Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
        var result = _client.Send(requestMessage);
        if (!result.IsSuccessStatusCode) return null;
        var responseContent = Helper.Deserialize<TOut>(result);
        return responseContent;
    }
}

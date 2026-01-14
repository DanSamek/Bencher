using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using Newtonsoft.Json;
using Shared;
using Shared.Dtos.Requests;
using Shared.Dtos.Responses;

namespace Worker;

/// <summary>
/// Implementation of the communication with the server.
/// </summary>
public class Communication
{
    private readonly RunnerOptions _options;
    private readonly string _workerMac;
    private readonly HttpClient _client;
    private readonly ErrorTrace _errorTrace;
    
    private static LoginResult INVALID_LOGIN = new LoginResult(false, string.Empty);
    
    /// <summary>
    /// .Ctor
    /// </summary>
    /// <param name="options"></param>
    public Communication(RunnerOptions options)
    {
        _options = options;
        _workerMac = GetWorkerMac();
        _client = SetupClient(); // Hopefully this won't waste all ports for the app ! TODO tests!
        _errorTrace = new ErrorTrace();
    }
    
    /// <summary>
    /// If error occured when request was send.
    /// </summary>
    public bool Error => _errorTrace.Error();

    /// <summary>
    /// Returns error message.
    /// </summary>
    public string GetErrorMessage() => _errorTrace.ToString();
    
    /// <summary>
    /// Implementation of the /get-test for autobench
    /// </summary>
    /// <returns>returns <see cref="Shared.Dtos.Responses.GetTestNonAutobenchResponse"/> if any test is in the queue, else null</returns>
    public GetTestNonAutobenchResponse? TryGetTest()
        => GetTest<GetTestNonAutobenchResponse>(false);
    
    /// <summary>
    /// Implementation of the /get-test for standard test.
    /// </summary>
    /// <returns>returns <see cref="Shared.Dtos.Responses.GetTestAutobenchResponse"/> if any test is in the queue, else null</returns>
    public GetTestAutobenchResponse? TryGetAutobenchTest()
        => GetTest<GetTestAutobenchResponse>(true);
    
    /// <summary>
    /// Notifies a server, that this workload is still running at the worker.
    /// </summary>
    /// <param name="connectionId">Connection id, that was obtained from <see cref="TryGetTest"/> <see cref="TryGetAutobenchTest"/></param>
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

    /// <summary>
    /// Sends autobench result to the server. 
    /// </summary>
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

    /// <summary>
    /// Sends pentanomial results to the server.
    /// </summary>
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
    
    /// <summary>
    /// Sends worker error to the server. 
    /// </summary>
    /// <param name="errorTrace">Trace with the error</param>
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
    
    /// <summary>
    /// Sends test error to the server. 
    /// </summary>
    /// <param name="errorTrace">Trace with the error</param>
    /// <param name="connectionId">Connection id, that was obtained from <see cref="TryGetTest"/> <see cref="TryGetAutobenchTest"/></param> 
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
    
    /// <summary>
    /// Result of the <see cref="TryLogin" />
    /// </summary>
    /// <param name="Success">If login was successful</param>
    /// <param name="Username">Username of the logged user</param>
    public record LoginResult(bool Success, string Username);
    
    /// <summary>
    /// Performs a try to login into web application worker-api.
    /// </summary>
    public static async Task<LoginResult> TryLogin(string userToken, string webApplicationUrl)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add(Constants.WORKER_REQUEST_HEADER, userToken);
            // The SSL connection could not be established, see inner exception --> dotnet dev-certs https --trust
            var response = await client.PostAsync($"{webApplicationUrl}/{Constants.WORKER_API_PREFIX}/validate", new StringContent(""));

            var result = Helper.Deserialize<ValidateResponseDto>(response);
            var loginResult = result?.Username is null
                ? INVALID_LOGIN
                : new LoginResult(Username: result.Username, Success: true);
            return loginResult;
        }
        catch
        {
            ApplicationInfo.ShowUnableToLogin(webApplicationUrl);
        }
        return INVALID_LOGIN;
    }

    private T? GetTest<T>(bool autobench)
        where T : class
    {
        try
        {
            var dto = CreateGetTestDto(autobench);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/{Constants.WORKER_API_PREFIX}/get-test");
            var responseContent = SendAndDeserialize<T, GetTestDto>(requestMessage, dto);
            return responseContent;
        }
        catch (Exception ex)
        {
            _errorTrace.AddError(ex.Message);
        }
        return null;
    }
    
    private HttpClient SetupClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add(Constants.WORKER_REQUEST_HEADER, _options.UserToken);
        client.BaseAddress = new Uri($"{_options.WebApplicationUrl}");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));;
        return client;
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

    private GetTestDto CreateGetTestDto(bool autobench)
    {
        var dto = new GetTestDto
        {
            Autobench = autobench,
            Mac = _workerMac,
            Name = Environment.MachineName,
            NumberOfThreads = _options.NumberOfThreads
        };
        return dto;
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
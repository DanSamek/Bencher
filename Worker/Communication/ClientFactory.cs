using System.Net.Http.Headers;
using Shared;

namespace Worker;

public class ClientFactory : IClientFactory
{
    private readonly RunnerOptions _options;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public ClientFactory(RunnerOptions options) => _options = options;
    
    /// <inheritdoc /> 
    public HttpClient Get()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add(Constants.WORKER_REQUEST_HEADER, _options.UserToken);
        client.BaseAddress = new Uri($"{_options.WebApplicationUrl}");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));;
        return client;
    }
}
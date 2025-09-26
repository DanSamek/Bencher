using log4net;

namespace WebApplication.API;

/// <summary>
/// Simple middleware, that is used for validations of the <see cref="Data.Models.ApplicationUser.AccessToken"/>.
/// Used only in /worker-api/* requests.
/// </summary>
public class WorkerMiddleware : IMiddleware
{
    private static readonly ILog _logger =  LogManager.GetLogger(typeof(WorkerMiddleware));
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var isWorkerApiRequest = context.Request.QueryString.Value?.Contains(Shared.WORKER_API_PREFIX) ?? false;
        if (isWorkerApiRequest) 
        {
            var result = HandleWorkerApiRequest(context);
            if (!result) return;
        }
        await next(context);
    }

    private static bool HandleWorkerApiRequest(HttpContext context)
    {
        var isTokenInHeaders = context.Request.Headers.ContainsKey(Shared.WORKER_REQUEST_HEADER);
        if (!isTokenInHeaders) return SetUnauthorized();
        
        var userStore = context.RequestServices.GetService<Stores.UserStore>();
        if (userStore is null)
        {
            _logger.Error($"{nameof(Stores.UserStore)} is null.");
            return SetUnauthorized();
        }
        
        var userToken = context.Request.Headers[Shared.WORKER_REQUEST_HEADER].ToString();
        var userTokenExists = userStore.DoesUserTokenExists(userToken);
        var result = userTokenExists || SetUnauthorized();
        return result;
        
        bool SetUnauthorized()
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return false;
        }
    }
}
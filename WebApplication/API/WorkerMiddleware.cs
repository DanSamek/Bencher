using log4net;
using Shared;

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
        var isWorkerApiRequest = context.Request.Path.Value?.Contains(Constants.WORKER_API_PREFIX) ?? false;
        if (isWorkerApiRequest) 
        {
            var result = HandleWorkerApiRequest(context);
            if (!result) return;
        }
        await next(context);
    }

    private static bool HandleWorkerApiRequest(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(Constants.WORKER_REQUEST_HEADER, out var userToken)) return SetUnauthorized();
        
        var userStore = context.RequestServices.GetService<Stores.UserStore>();
        if (userStore is null)
        {
            _logger.Error($"{nameof(Stores.UserStore)} is null.");
            return SetUnauthorized();
        }
        
        var userTokenExists = userStore.DoesUserTokenExists(userToken.ToString());
        var result = userTokenExists || SetUnauthorized();
        return result;
        
        bool SetUnauthorized()
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return false;
        }
    }
}
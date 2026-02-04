using System.Text.Json;
using Shared.Dtos.Responses;

namespace WebApplication.API;

/// <summary>
/// Custom exception handler for the controller.
/// </summary>
public class CustomExceptionHandler : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }   
        catch (NotFoundException)
        {
            var error = new ResponseBase();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(error);
        }
    }
}
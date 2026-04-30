using System.Text.Json;
using TaskManager.Application.Common;
using TaskManager.Domain.Exceptions;

namespace TaskManager.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, message, errors) = exception switch
        {
            AppException appEx when appEx is Domain.Exceptions.ValidationException validEx =>
                (400, validEx.Message, validEx.Errors),
            AppException appEx => (appEx.StatusCode, appEx.Message, (IEnumerable<string>?)null),
            _ => (500, "An unexpected error occurred.", (IEnumerable<string>?)null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Fail(message, errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

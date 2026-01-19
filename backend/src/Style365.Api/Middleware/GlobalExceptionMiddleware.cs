using System.Net;
using System.Text.Json;
using Style365.Application.Common.Exceptions;

namespace Style365.Api.Middleware;

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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            message = "An error occurred while processing your request.",
            details = (string?)null,
            errors = new List<string>()
        };

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    message = "Validation failed",
                    details = (string?)validationEx.Message,
                    errors = validationEx.Errors.ToList()
                };
                break;

            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    message = "Resource not found",
                    details = (string?)notFoundEx.Message,
                    errors = new List<string>()
                };
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = new
                {
                    message = "Unauthorized access",
                    details = (string?)"You do not have permission to access this resource.",
                    errors = new List<string>()
                };
                break;

            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    message = "Invalid argument",
                    details = (string?)argEx.Message,
                    errors = new List<string>()
                };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new
                {
                    message = "An internal server error occurred",
                    details = (string?)"Please try again later or contact support if the problem persists.",
                    errors = new List<string>()
                };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
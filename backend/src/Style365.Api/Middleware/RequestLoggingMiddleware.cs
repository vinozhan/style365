using System.Diagnostics;

namespace Style365.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();
        
        // Add request ID to the response headers
        context.Response.Headers["X-Request-Id"] = requestId;
        
        // Log request
        _logger.LogInformation(
            "HTTP {Method} {Path} started - RequestId: {RequestId} - UserAgent: {UserAgent} - RemoteIP: {RemoteIP}",
            context.Request.Method,
            context.Request.Path,
            requestId,
            context.Request.Headers.UserAgent.ToString(),
            GetClientIP(context)
        );

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Log response
            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms - RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestId
            );
        }
    }

    private static string GetClientIP(HttpContext context)
    {
        return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
               ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
               ?? context.Connection.RemoteIpAddress?.ToString()
               ?? "Unknown";
    }
}
using MediatR;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Style365.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var uniqueId = Guid.NewGuid().ToString();

        _logger.LogInformation("Style365 Request: {Name} {@UserId} {@UserName} {@Request} {RequestId}",
            requestName, string.Empty, string.Empty, request, uniqueId);

        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next();

            sw.Stop();
            _logger.LogInformation("Style365 Request completed: {Name} {RequestId} {ElapsedMilliseconds}ms",
                requestName, uniqueId, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Style365 Request failed: {Name} {RequestId} {ElapsedMilliseconds}ms",
                requestName, uniqueId, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
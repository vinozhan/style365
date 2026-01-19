using Microsoft.Extensions.Diagnostics.HealthChecks;
using Style365.Infrastructure.Data;

namespace Style365.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly Style365DbContext _context;

    public DatabaseHealthCheck(Style365DbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to execute a simple query to check database connectivity
            await _context.Database.CanConnectAsync(cancellationToken);
            
            return HealthCheckResult.Healthy("Database is accessible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is not accessible", ex);
        }
    }
}
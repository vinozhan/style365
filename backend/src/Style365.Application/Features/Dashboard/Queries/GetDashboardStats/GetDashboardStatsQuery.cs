using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Dashboard.Queries.GetDashboardStats;

public record GetDashboardStatsQuery : IQuery<Result<DashboardStatsDto>>;

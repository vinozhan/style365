namespace Style365.Application.Features.Dashboard.Queries.GetDashboardStats;

public record DashboardStatsDto
{
    public int TotalOrders { get; init; }
    public decimal TotalRevenue { get; init; }
    public int TotalProducts { get; init; }
    public int LowStockProducts { get; init; }
    public int TotalCustomers { get; init; }
    public int PendingOrders { get; init; }
    public int ProcessingOrders { get; init; }
    public int CompletedOrders { get; init; }
    public IEnumerable<RecentOrderDto> RecentOrders { get; init; } = [];
    public IEnumerable<TopProductDto> TopProducts { get; init; } = [];
}

public record RecentOrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record TopProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int TotalSold { get; init; }
    public decimal Revenue { get; init; }
}

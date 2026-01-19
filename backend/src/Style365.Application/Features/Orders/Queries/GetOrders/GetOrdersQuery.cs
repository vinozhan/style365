using MediatR;
using Style365.Application.Common.Models;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQuery : IRequest<Result<PaginatedResult<OrderSummaryDto>>>
{
    public Guid? UserId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class OrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public string? TrackingNumber { get; set; }
    public int ItemCount { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
}
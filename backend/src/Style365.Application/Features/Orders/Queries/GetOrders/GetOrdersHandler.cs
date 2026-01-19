using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, Result<PaginatedResult<OrderSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrdersHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedResult<OrderSummaryDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Order> orders;

        // Get orders based on filters
        if (request.UserId.HasValue)
        {
            orders = await _unitOfWork.Orders.GetOrdersByUserAsync(request.UserId.Value, cancellationToken);
        }
        else if (request.Status.HasValue)
        {
            orders = await _unitOfWork.Orders.GetOrdersByStatusAsync(request.Status.Value, cancellationToken);
        }
        else if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            orders = await _unitOfWork.Orders.GetOrdersByDateRangeAsync(request.StartDate.Value, request.EndDate.Value, cancellationToken);
        }
        else
        {
            orders = await _unitOfWork.Orders.GetAllAsync();
        }

        // Apply additional filtering
        if (request.Status.HasValue && !request.UserId.HasValue)
        {
            orders = orders.Where(o => o.Status == request.Status.Value);
        }

        if (request.StartDate.HasValue)
        {
            orders = orders.Where(o => o.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            orders = orders.Where(o => o.CreatedAt <= request.EndDate.Value);
        }

        // Apply sorting
        switch (request.SortBy?.ToLower())
        {
            case "ordernumber":
                orders = request.SortDescending ? orders.OrderByDescending(o => o.OrderNumber) : orders.OrderBy(o => o.OrderNumber);
                break;
            case "status":
                orders = request.SortDescending ? orders.OrderByDescending(o => o.Status) : orders.OrderBy(o => o.Status);
                break;
            case "totalamount":
                orders = request.SortDescending ? orders.OrderByDescending(o => o.TotalAmount.Amount) : orders.OrderBy(o => o.TotalAmount.Amount);
                break;
            default:
                orders = request.SortDescending ? orders.OrderByDescending(o => o.CreatedAt) : orders.OrderBy(o => o.CreatedAt);
                break;
        }

        // Apply pagination
        var totalCount = orders.Count();
        var skip = (request.Page - 1) * request.PageSize;
        var pagedOrders = orders.Skip(skip).Take(request.PageSize).ToList();

        // Map to DTOs
        var orderDtos = pagedOrders.Select(order => new OrderSummaryDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            CreatedAt = order.CreatedAt,
            ShippedAt = order.ShippedAt,
            TrackingNumber = order.TrackingNumber,
            ItemCount = order.Items.Count,
            CustomerEmail = order.CustomerEmail
        }).ToList();

        return Result.Success(PaginatedResult<OrderSummaryDto>.Create(
            orderDtos,
            request.Page,
            request.PageSize,
            totalCount
        ));
    }
}
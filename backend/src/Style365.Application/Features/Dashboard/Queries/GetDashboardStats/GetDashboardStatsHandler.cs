using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsHandler : IQueryHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardStatsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DashboardStatsDto>> Handle(
        GetDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get order statistics
            var totalOrders = await _unitOfWork.Orders.GetOrderCountAsync(cancellationToken: cancellationToken);
            var totalRevenue = await _unitOfWork.Orders.GetTotalSalesAsync(cancellationToken: cancellationToken);

            // Get orders by status
            var pendingOrders = await _unitOfWork.Orders.GetOrdersByStatusAsync(OrderStatus.Pending, cancellationToken);
            var processingOrders = await _unitOfWork.Orders.GetOrdersByStatusAsync(OrderStatus.Processing, cancellationToken);
            var deliveredOrders = await _unitOfWork.Orders.GetOrdersByStatusAsync(OrderStatus.Delivered, cancellationToken);

            // Get product statistics
            var allProducts = await _unitOfWork.Products.GetAllAsync(cancellationToken);
            var lowStockProducts = await _unitOfWork.Products.GetLowStockProductsAsync(cancellationToken);

            // Get customer count (users with Customer role)
            var customers = await _unitOfWork.Users.GetUsersByRoleAsync(UserRole.Customer, cancellationToken);

            // Get recent orders
            var recentOrders = await _unitOfWork.Orders.GetRecentOrdersAsync(5, cancellationToken);
            var recentOrderDtos = new List<RecentOrderDto>();

            foreach (var order in recentOrders)
            {
                string customerName = "Guest";
                string customerEmail = order.CustomerEmail;

                if (order.UserId.HasValue)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(order.UserId.Value, cancellationToken);
                    if (user != null)
                    {
                        customerName = $"{user.FirstName} {user.LastName}";
                        customerEmail = user.Email.Value;
                    }
                }

                recentOrderDtos.Add(new RecentOrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = customerName,
                    CustomerEmail = customerEmail,
                    TotalAmount = order.TotalAmount.Amount,
                    Status = order.Status.ToString(),
                    CreatedAt = order.CreatedAt
                });
            }

            // Get top selling products (simplified - based on order items)
            var topOrders = await _unitOfWork.Orders.GetTopOrdersByValueAsync(10, cancellationToken);
            var productSales = new Dictionary<Guid, (string Name, string? ImageUrl, int Quantity, decimal Revenue)>();

            foreach (var order in topOrders)
            {
                var orderWithItems = await _unitOfWork.Orders.GetByIdWithItemsAsync(order.Id, cancellationToken);
                if (orderWithItems?.Items != null)
                {
                    foreach (var item in orderWithItems.Items)
                    {
                        var lineTotal = item.GetLineTotal().Amount;

                        if (productSales.TryGetValue(item.ProductId, out var existing))
                        {
                            productSales[item.ProductId] = (
                                existing.Name,
                                existing.ImageUrl,
                                existing.Quantity + item.Quantity,
                                existing.Revenue + lineTotal
                            );
                        }
                        else
                        {
                            var product = await _unitOfWork.Products.GetByIdWithImagesAsync(item.ProductId, cancellationToken);
                            productSales[item.ProductId] = (
                                item.ProductName,
                                product?.Images.FirstOrDefault()?.ImageUrl,
                                item.Quantity,
                                lineTotal
                            );
                        }
                    }
                }
            }

            var topProducts = productSales
                .OrderByDescending(p => p.Value.Revenue)
                .Take(5)
                .Select(p => new TopProductDto
                {
                    Id = p.Key,
                    Name = p.Value.Name,
                    ImageUrl = p.Value.ImageUrl,
                    TotalSold = p.Value.Quantity,
                    Revenue = p.Value.Revenue
                })
                .ToList();

            var stats = new DashboardStatsDto
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalProducts = allProducts.Count(),
                LowStockProducts = lowStockProducts.Count(),
                TotalCustomers = customers.Count(),
                PendingOrders = pendingOrders.Count(),
                ProcessingOrders = processingOrders.Count(),
                CompletedOrders = deliveredOrders.Count(),
                RecentOrders = recentOrderDtos,
                TopProducts = topProducts
            };

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            return Result.Failure<DashboardStatsDto>($"Failed to retrieve dashboard stats: {ex.Message}");
        }
    }
}

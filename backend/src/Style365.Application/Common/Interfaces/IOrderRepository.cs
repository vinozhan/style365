using Style365.Domain.Entities;
using Style365.Domain.Enums;

namespace Style365.Application.Common.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithPaymentsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOrdersByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default);
    Task<bool> IsOrderNumberUniqueAsync(string orderNumber, Guid? excludeOrderId = null, CancellationToken cancellationToken = default);
    Task<Order?> GetLastOrderByYearAsync(int year, CancellationToken cancellationToken = default);
    
    // Analytics methods
    Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<int> GetOrderCountAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetTopOrdersByValueAsync(int count, CancellationToken cancellationToken = default);
}
using Microsoft.EntityFrameworkCore;
using Style365.Domain.Entities;
using Style365.Domain.Enums;
using Style365.Infrastructure.Data;
using Style365.Application.Common.Interfaces;

namespace Style365.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(Style365DbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.Items)
            .ThenInclude(oi => oi.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByIdWithPaymentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByIdWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.Items)
            .ThenInclude(oi => oi.ProductVariant)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsOrderNumberUniqueAsync(string orderNumber, Guid? excludeOrderId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(o => o.OrderNumber == orderNumber);
        
        if (excludeOrderId.HasValue)
            query = query.Where(o => o.Id != excludeOrderId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(o => o.Status == OrderStatus.Delivered);

        if (startDate.HasValue)
            query = query.Where(o => o.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.CreatedAt <= endDate.Value);

        return await query.SumAsync(o => o.TotalAmount.Amount, cancellationToken);
    }

    public async Task<int> GetOrderCountAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(o => o.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.CreatedAt <= endDate.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetTopOrdersByValueAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.User)
            .OrderByDescending(o => o.TotalAmount.Amount)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetLastOrderByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.OrderNumber.Contains($"ORD-{year}-"))
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
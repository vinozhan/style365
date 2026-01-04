using Microsoft.EntityFrameworkCore;
using Style365.Domain.Entities;
using Style365.Domain.Enums;
using Style365.Infrastructure.Data;
using Style365.Infrastructure.Repositories.Interfaces;

namespace Style365.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(Style365DbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetByPaymentReferenceAsync(string paymentReference, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.PaymentReference == paymentReference, cancellationToken);
    }

    public async Task<Payment?> GetByGatewayTransactionIdAsync(string gatewayTransactionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.PaymentGatewayTransactionId == gatewayTransactionId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == status)
            .Include(p => p.Order)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(PaymentMethod method, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Method == method)
            .Include(p => p.Order)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
            .Include(p => p.Order)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == PaymentStatus.Failed)
            .Include(p => p.Order)
            .OrderByDescending(p => p.FailedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetRefundedPaymentsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == PaymentStatus.Refunded || p.Status == PaymentStatus.PartialRefund)
            .Include(p => p.Order)
            .OrderByDescending(p => p.RefundedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsPaymentReferenceUniqueAsync(string paymentReference, Guid? excludePaymentId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.PaymentReference == paymentReference);
        
        if (excludePaymentId.HasValue)
            query = query.Where(p => p.Id != excludePaymentId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Status == PaymentStatus.Completed);

        if (startDate.HasValue)
            query = query.Where(p => p.ProcessedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.ProcessedAt <= endDate.Value);

        return await query.SumAsync(p => p.Amount.Amount, cancellationToken);
    }

    public async Task<decimal> GetTotalRefundsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.RefundedAmount != null);

        if (startDate.HasValue)
            query = query.Where(p => p.RefundedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.RefundedAt <= endDate.Value);

        return await query.SumAsync(p => p.RefundedAmount!.Amount, cancellationToken);
    }

    public async Task<int> GetPaymentCountByStatusAsync(PaymentStatus status, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Status == status);

        if (startDate.HasValue)
            query = query.Where(p => p.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.CreatedAt <= endDate.Value);

        return await query.CountAsync(cancellationToken);
    }
}
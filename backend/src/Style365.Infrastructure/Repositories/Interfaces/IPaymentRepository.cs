using Style365.Domain.Entities;
using Style365.Domain.Enums;

namespace Style365.Infrastructure.Repositories.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByPaymentReferenceAsync(string paymentReference, CancellationToken cancellationToken = default);
    Task<Payment?> GetByGatewayTransactionIdAsync(string gatewayTransactionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPaymentsByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(PaymentMethod method, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetFailedPaymentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetRefundedPaymentsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsPaymentReferenceUniqueAsync(string paymentReference, Guid? excludePaymentId = null, CancellationToken cancellationToken = default);
    
    // Analytics methods
    Task<decimal> GetTotalPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRefundsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<int> GetPaymentCountByStatusAsync(PaymentStatus status, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}
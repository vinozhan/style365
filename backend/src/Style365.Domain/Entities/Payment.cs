using Style365.Domain.Common;
using Style365.Domain.Enums;
using Style365.Domain.ValueObjects;

namespace Style365.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; private set; }
    public string PaymentReference { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? PaymentGatewayTransactionId { get; private set; }
    public string? PaymentGatewayResponse { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? RefundReference { get; private set; }
    public Money? RefundedAmount { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    public Order Order { get; private set; } = null!;

    private Payment() { }

    public Payment(Guid orderId, Money amount, PaymentMethod method)
    {
        OrderId = orderId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
        PaymentReference = GeneratePaymentReference();
    }

    public void MarkAsProcessing(string? gatewayTransactionId = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be marked as processing");

        Status = PaymentStatus.Processing;
        PaymentGatewayTransactionId = gatewayTransactionId?.Trim();
        UpdateTimestamp();
    }

    public void MarkAsCompleted(string? gatewayResponse = null)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException("Only processing payments can be completed");

        Status = PaymentStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
        PaymentGatewayResponse = gatewayResponse?.Trim();
        UpdateTimestamp();
    }

    public void MarkAsFailed(string failureReason, string? gatewayResponse = null)
    {
        if (Status == PaymentStatus.Completed || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot mark completed or refunded payments as failed");

        Status = PaymentStatus.Failed;
        FailedAt = DateTime.UtcNow;
        FailureReason = failureReason?.Trim();
        PaymentGatewayResponse = gatewayResponse?.Trim();
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == PaymentStatus.Completed || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot cancel completed or refunded payments");

        Status = PaymentStatus.Cancelled;
        UpdateTimestamp();
    }

    public void ProcessRefund(Money refundAmount, string refundReference)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Only completed payments can be refunded");

        if (refundAmount.Amount <= 0)
            throw new ArgumentException("Refund amount must be positive");

        var totalRefunded = RefundedAmount?.Amount ?? 0;
        if (totalRefunded + refundAmount.Amount > Amount.Amount)
            throw new ArgumentException("Total refund cannot exceed payment amount");

        RefundedAmount = RefundedAmount?.Add(refundAmount) ?? refundAmount;
        RefundReference = refundReference?.Trim();
        RefundedAt = DateTime.UtcNow;

        if (RefundedAmount.Amount >= Amount.Amount)
        {
            Status = PaymentStatus.Refunded;
        }
        else
        {
            Status = PaymentStatus.PartialRefund;
        }

        UpdateTimestamp();
    }

    public bool IsFullyRefunded() => RefundedAmount?.Amount >= Amount.Amount;

    private static string GeneratePaymentReference()
    {
        return $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}
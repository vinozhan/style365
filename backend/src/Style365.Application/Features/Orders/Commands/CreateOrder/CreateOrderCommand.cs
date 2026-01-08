using MediatR;
using Style365.Application.Common.Models;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<Result<CreateOrderResponse>>
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public OrderShippingInfo ShippingAddress { get; set; } = null!;
    public OrderShippingInfo? BillingAddress { get; set; } // If null, use shipping address
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentReference { get; set; } // Payment gateway reference
    public string? OrderNotes { get; set; }
    public string? CouponCode { get; set; }
}

public class OrderShippingInfo
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class CreateOrderResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? PaymentUrl { get; set; } // For redirecting to payment gateway
}
using MediatR;
using Style365.Application.Common.Models;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQuery : IRequest<Result<OrderDto>>
{
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public Guid? UserId { get; set; } // For authorization check
    public string? CustomerEmail { get; set; } // For order tracking without auth
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ShippingCarrier { get; set; }
    public string? Notes { get; set; }
    
    public OrderAddressDto ShippingAddress { get; set; } = null!;
    public OrderAddressDto BillingAddress { get; set; } = null!;
    public List<OrderItemDto> Items { get; set; } = new();
    public List<OrderPaymentDto> Payments { get; set; } = new();
}

public class OrderAddressDto
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

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public Guid? VariantId { get; set; }
    public string? VariantName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class OrderPaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? Reference { get; set; }
    public DateTime CreatedAt { get; set; }
}
using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Cart.Queries.GetCart;

public class GetCartQuery : IRequest<Result<CartDto>>
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
}

public class CartDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime LastModified { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public Guid? VariantId { get; set; }
    public string? VariantName { get; set; }
    public string? VariantColor { get; set; }
    public string? VariantSize { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime AddedAt { get; set; }
}
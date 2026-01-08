using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Cart.Commands.AddToCart;

public class AddToCartCommand : IRequest<Result<AddToCartResponse>>
{
    public Guid? UserId { get; set; } // Null for guest cart
    public string? SessionId { get; set; } // For guest cart identification
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; } // Optional product variant
    public int Quantity { get; set; } = 1;
}

public class AddToCartResponse
{
    public Guid CartId { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
    public string Message { get; set; } = "Item added to cart";
}
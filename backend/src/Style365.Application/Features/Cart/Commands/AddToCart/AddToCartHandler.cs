using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
// using Cart = Style365.Domain.Entities.ShoppingCart;

namespace Style365.Application.Features.Cart.Commands.AddToCart;

public class AddToCartHandler : IRequestHandler<AddToCartCommand, Result<AddToCartResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddToCartHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AddToCartResponse>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Verify product exists and is active
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null || !product.IsActive)
        {
            return Result.Failure<AddToCartResponse>("Product not found or not available");
        }

        // Verify variant if specified
        ProductVariant? variant = null;
        if (request.VariantId.HasValue)
        {
            var productWithVariants = await _unitOfWork.Products.GetByIdWithVariantsAsync(request.ProductId, cancellationToken);
            variant = productWithVariants?.Variants.FirstOrDefault(v => v.Id == request.VariantId.Value);
            
            if (variant == null || !variant.IsActive)
            {
                return Result.Failure<AddToCartResponse>("Product variant not found or not available");
            }
        }

        // Check stock availability
        var availableStock = variant?.StockQuantity ?? product.StockQuantity;
        if (product.TrackQuantity && availableStock < request.Quantity)
        {
            return Result.Failure<AddToCartResponse>($"Only {availableStock} items available in stock");
        }

        // Find or create shopping cart
        ShoppingCart cart;
        if (request.UserId.HasValue)
        {
            cart = await _unitOfWork.ShoppingCarts.GetByUserIdAsync(request.UserId.Value, cancellationToken) ??
                   new ShoppingCart(request.UserId.Value);
        }
        else
        {
            cart = await _unitOfWork.ShoppingCarts.GetBySessionIdAsync(request.SessionId!, cancellationToken) ??
                   new ShoppingCart(request.SessionId!);
        }

        // Check if item already exists in cart
        var existingItem = cart.Items.FirstOrDefault(i => 
            i.ProductId == request.ProductId && i.ProductVariantId == request.VariantId);

        if (existingItem != null)
        {
            // Update quantity of existing item
            var newQuantity = existingItem.Quantity + request.Quantity;
            
            // Check total stock for new quantity
            if (product.TrackQuantity && availableStock < newQuantity)
            {
                return Result.Failure<AddToCartResponse>($"Cannot add {request.Quantity} more items. Only {availableStock - existingItem.Quantity} additional items available");
            }
            
            existingItem.UpdateQuantity(newQuantity);
        }
        else
        {
            // Add new item to cart
            var unitPrice = variant?.Price ?? product.Price;
            var cartItem = new CartItem(
                request.ProductId,
                request.VariantId,
                request.Quantity,
                unitPrice
            );
            
            cart.AddItem(cartItem);
        }

        // Save cart
        if (cart.Id == Guid.Empty)
        {
            await _unitOfWork.ShoppingCarts.AddAsync(cart, cancellationToken);
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AddToCartResponse
        {
            CartId = cart.Id,
            TotalItems = cart.Items.Sum(i => i.Quantity),
            TotalAmount = cart.GetTotal().Amount
        };

        return Result.Success(response);
    }
}
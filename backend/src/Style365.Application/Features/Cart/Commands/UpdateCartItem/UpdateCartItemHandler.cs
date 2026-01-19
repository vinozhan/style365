using MediatR;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Cart.Commands.UpdateCartItem;

public class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartItemHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return Result.Failure("Quantity must be greater than 0");
        }

        // Find the cart
        ShoppingCart? cart = null;
        if (request.UserId.HasValue)
        {
            cart = await _unitOfWork.ShoppingCarts.GetByUserIdWithItemsAsync(request.UserId.Value, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.SessionId))
        {
            cart = await _unitOfWork.ShoppingCarts.GetBySessionIdWithItemsAsync(request.SessionId, cancellationToken);
        }

        if (cart == null)
        {
            return Result.Failure("Shopping cart not found");
        }

        // Find the cart item
        var cartItem = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId);
        if (cartItem == null)
        {
            throw new NotFoundException(nameof(CartItem), request.CartItemId);
        }

        // Check stock availability
        var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(cartItem.ProductId, cancellationToken);
        if (product == null || !product.IsActive)
        {
            return Result.Failure("Product is no longer available");
        }

        var availableStock = cartItem.ProductVariantId.HasValue
            ? product.Variants.FirstOrDefault(v => v.Id == cartItem.ProductVariantId)?.StockQuantity ?? 0
            : product.StockQuantity;

        if (product.TrackQuantity && availableStock < request.Quantity)
        {
            return Result.Failure($"Only {availableStock} items available in stock");
        }

        // Update quantity
        cart.UpdateItemQuantity(cartItem.ProductId, request.Quantity, cartItem.ProductVariantId);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
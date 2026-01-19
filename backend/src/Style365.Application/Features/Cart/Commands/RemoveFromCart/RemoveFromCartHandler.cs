using MediatR;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Cart.Commands.RemoveFromCart;

public class RemoveFromCartHandler : IRequestHandler<RemoveFromCartCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFromCartHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
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

        // Remove the item
        cart.RemoveItem(cartItem.ProductId, cartItem.ProductVariantId);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
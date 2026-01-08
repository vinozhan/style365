using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Cart.Commands.ClearCart;

public class ClearCartHandler : IRequestHandler<ClearCartCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public ClearCartHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
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
            return Result.Success(); // Cart doesn't exist, nothing to clear
        }

        // Clear the cart
        cart.ClearCart();
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
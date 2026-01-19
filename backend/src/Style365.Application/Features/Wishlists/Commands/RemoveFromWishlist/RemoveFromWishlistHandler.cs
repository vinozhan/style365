using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Wishlists.Commands.RemoveFromWishlist;

public class RemoveFromWishlistHandler : IRequestHandler<RemoveFromWishlistCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFromWishlistHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        // Get the specific wishlist or default wishlist
        var wishlist = request.WishlistId.HasValue
            ? await _unitOfWork.Wishlists.GetByIdWithItemsAsync(request.WishlistId.Value, cancellationToken)
            : await _unitOfWork.Wishlists.GetDefaultByUserIdAsync(request.UserId, cancellationToken);

        if (wishlist == null)
        {
            return Result.Failure("Wishlist not found");
        }

        // Verify the wishlist belongs to the user
        if (wishlist.UserId != request.UserId)
        {
            return Result.Failure("Access denied");
        }

        // Find the item to remove
        var itemToRemove = wishlist.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (itemToRemove == null)
        {
            return Result.Failure("Product not found in wishlist");
        }

        // Remove the item from the wishlist
        wishlist.RemoveProduct(itemToRemove.ProductId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
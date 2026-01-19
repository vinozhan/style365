using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Wishlists.Commands.AddToWishlist;

public class AddToWishlistHandler : IRequestHandler<AddToWishlistCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddToWishlistHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        // Verify product exists
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Failure("Product not found");
        }

        Wishlist? wishlist;

        if (request.WishlistId.HasValue)
        {
            // Use specified wishlist
            wishlist = await _unitOfWork.Wishlists.GetByIdWithItemsAsync(request.WishlistId.Value, cancellationToken);
            if (wishlist == null || wishlist.UserId != request.UserId)
            {
                return Result.Failure("Wishlist not found or access denied");
            }
        }
        else
        {
            // Use default wishlist or create one
            wishlist = await _unitOfWork.Wishlists.GetDefaultByUserIdAsync(request.UserId, cancellationToken);
            if (wishlist == null)
            {
                wishlist = new Wishlist(request.UserId, "My Wishlist", true);
                await _unitOfWork.Wishlists.AddAsync(wishlist, cancellationToken);
            }
        }

        // Check if product is already in wishlist
        if (wishlist.ContainsProduct(request.ProductId))
        {
            return Result.Failure("Product is already in your wishlist");
        }

        // Add product to wishlist
        wishlist.AddProduct(request.ProductId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
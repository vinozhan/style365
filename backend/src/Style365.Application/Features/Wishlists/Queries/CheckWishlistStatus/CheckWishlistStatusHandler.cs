using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Wishlists.Queries.CheckWishlistStatus;

public class CheckWishlistStatusHandler : IRequestHandler<CheckWishlistStatusQuery, Result<CheckWishlistStatusResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CheckWishlistStatusHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CheckWishlistStatusResponse>> Handle(CheckWishlistStatusQuery request, CancellationToken cancellationToken)
    {
        // Get all wishlists containing the product for this user
        var allWishlistsWithProduct = await _unitOfWork.Wishlists.GetWishlistsContainingProductAsync(request.ProductId, cancellationToken);
        var userWishlistsWithProduct = allWishlistsWithProduct.Where(w => w.UserId == request.UserId).ToList();

        var response = new CheckWishlistStatusResponse
        {
            IsInWishlist = userWishlistsWithProduct.Any(),
            WishlistIds = userWishlistsWithProduct.Select(w => w.Id).ToList()
        };

        return Result.Success(response);
    }
}
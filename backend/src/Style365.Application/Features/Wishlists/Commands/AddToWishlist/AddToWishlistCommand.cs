using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Wishlists.Commands.AddToWishlist;

public class AddToWishlistCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? WishlistId { get; set; } // If null, use default wishlist
}
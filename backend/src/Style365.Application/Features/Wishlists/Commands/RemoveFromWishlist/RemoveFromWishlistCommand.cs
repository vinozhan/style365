using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Wishlists.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? WishlistId { get; set; }
}
using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Wishlists.Queries.CheckWishlistStatus;

public class CheckWishlistStatusQuery : IRequest<Result<CheckWishlistStatusResponse>>
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
}
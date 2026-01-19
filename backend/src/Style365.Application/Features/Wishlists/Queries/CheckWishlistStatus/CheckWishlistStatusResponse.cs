namespace Style365.Application.Features.Wishlists.Queries.CheckWishlistStatus;

public class CheckWishlistStatusResponse
{
    public bool IsInWishlist { get; set; }
    public List<Guid> WishlistIds { get; set; } = new();
}
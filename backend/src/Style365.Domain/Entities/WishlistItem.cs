using Style365.Domain.Common;

namespace Style365.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime AddedAt { get; private set; }

    public Wishlist Wishlist { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private WishlistItem() { }

    public WishlistItem(Guid wishlistId, Guid productId)
    {
        WishlistId = wishlistId;
        ProductId = productId;
        AddedAt = DateTime.UtcNow;
    }
}
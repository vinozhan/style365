using Style365.Domain.Common;

namespace Style365.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime AddedAt { get; private set; }

    public ShoppingCart Cart { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public ProductVariant? ProductVariant { get; private set; }

    private CartItem() { }

    public CartItem(Guid cartId, Guid productId, int quantity, Guid? variantId = null)
    {
        CartId = cartId;
        ProductId = productId;
        ProductVariantId = variantId;
        Quantity = ValidateQuantity(quantity);
        AddedAt = DateTime.UtcNow;
    }

    public void UpdateQuantity(int quantity)
    {
        Quantity = ValidateQuantity(quantity);
        UpdateTimestamp();
    }

    private static int ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        return quantity;
    }
}
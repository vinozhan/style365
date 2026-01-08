using Style365.Domain.Common;
using Style365.Domain.ValueObjects;

namespace Style365.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public DateTime AddedAt { get; private set; }

    public ShoppingCart Cart { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public ProductVariant? ProductVariant { get; private set; }

    private CartItem() { }

    public CartItem(Guid productId, Guid? variantId, int quantity, Money unitPrice)
    {
        ProductId = productId;
        ProductVariantId = variantId;
        Quantity = ValidateQuantity(quantity);
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        AddedAt = DateTime.UtcNow;
    }

    public CartItem(Guid cartId, Guid productId, int quantity, Guid? variantId = null)
    {
        CartId = cartId;
        ProductId = productId;
        ProductVariantId = variantId;
        Quantity = ValidateQuantity(quantity);
        AddedAt = DateTime.UtcNow;
        // Note: UnitPrice needs to be set separately when using this constructor
    }

    public void UpdateQuantity(int quantity)
    {
        Quantity = ValidateQuantity(quantity);
        UpdateTimestamp();
    }

    public Money GetSubtotal()
    {
        return Money.Create(UnitPrice.Amount * Quantity, UnitPrice.Currency);
    }

    public void UpdateUnitPrice(Money unitPrice)
    {
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        UpdateTimestamp();
    }

    private static int ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        return quantity;
    }
}
using Style365.Domain.Common;
using Style365.Domain.ValueObjects;

namespace Style365.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public string ProductSku { get; private set; } = null!;
    public string? VariantName { get; private set; }
    public string? ProductImage { get; private set; }

    public Order Order { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public ProductVariant? ProductVariant { get; private set; }

    private OrderItem() { }
  

    public OrderItem(Guid productId, Guid? variantId, int quantity, Money unitPrice)
    {
        ProductId = productId;
        ProductVariantId = variantId;
        Quantity = ValidateQuantity(quantity);
        UnitPrice = unitPrice;
        ProductName = string.Empty;
        ProductSku = string.Empty;
    }

    public OrderItem(Guid orderId, Guid productId, int quantity, Money unitPrice, Guid? variantId = null)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductVariantId = variantId;
        Quantity = ValidateQuantity(quantity);
        UnitPrice = unitPrice;
        ProductName = string.Empty;
        ProductSku = string.Empty;
    }

    public void SetProductDetails(string productName, string productSku, string? variantName = null, string? productImage = null)
    {
        ProductName = ValidateName(productName, nameof(productName));
        ProductSku = ValidateSku(productSku);
        VariantName = variantName?.Trim();
        ProductImage = productImage?.Trim();
        UpdateTimestamp();
    }

    public void UpdateQuantity(int quantity)
    {
        Quantity = ValidateQuantity(quantity);
        UpdateTimestamp();
    }

    public void UpdatePrice(Money unitPrice)
    {
        UnitPrice = unitPrice;
        UpdateTimestamp();
    }

    public Money GetLineTotal() => UnitPrice * Quantity;

    private static int ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        return quantity;
    }

    private static string ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", paramName);
        return name.Trim();
    }

    private static string ValidateSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));
        return sku.Trim().ToUpperInvariant();
    }
}
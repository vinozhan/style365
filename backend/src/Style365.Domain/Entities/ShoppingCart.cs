using Style365.Domain.Common;
using Style365.Domain.ValueObjects;

namespace Style365.Domain.Entities;

public class ShoppingCart : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string? SessionId { get; private set; }
    public DateTime LastModified { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private readonly List<CartItem> _items = [];
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    private ShoppingCart() { }

    public ShoppingCart(Guid userId)
    {
        UserId = userId;
        SessionId = null;
        LastModified = DateTime.UtcNow;
        ExpiresAt = null; // User carts don't expire
    }

    public ShoppingCart(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId cannot be empty", nameof(sessionId));

        UserId = null;
        SessionId = sessionId.Trim();
        LastModified = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddDays(7); // Guest carts expire in 7 days
    }

    public void AddItem(CartItem cartItem)
    {
        if (cartItem == null)
            throw new ArgumentNullException(nameof(cartItem));

        _items.Add(cartItem);
        UpdateLastModified();
    }

    public void AddItem(Guid productId, int quantity, Guid? variantId = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == variantId);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var cartItem = new CartItem(Id, productId, quantity, variantId);
            _items.Add(cartItem);
        }

        UpdateLastModified();
    }

    public Money GetTotal()
    {
        if (!_items.Any())
            return Money.Create(0, "USD"); // Default currency

        var total = _items.Sum(item => item.GetSubtotal().Amount);
        var currency = _items.First().UnitPrice.Currency;
        return Money.Create(total, currency);
    }

    public void UpdateItemQuantity(Guid productId, int newQuantity, Guid? variantId = null)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == variantId);
        
        if (item == null)
            throw new ArgumentException("Item not found in cart");

        if (newQuantity <= 0)
        {
            RemoveItem(productId, variantId);
        }
        else
        {
            item.UpdateQuantity(newQuantity);
            UpdateLastModified();
        }
    }

    public void RemoveItem(Guid productId, Guid? variantId = null)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == variantId);
        
        if (item != null)
        {
            _items.Remove(item);
            UpdateLastModified();
        }
    }

    public void ClearCart()
    {
        _items.Clear();
        UpdateLastModified();
    }

    public void MergeCart(ShoppingCart otherCart)
    {
        foreach (var item in otherCart.Items)
        {
            AddItem(item.ProductId, item.Quantity, item.ProductVariantId);
        }
    }

    public void AssignToUser(Guid userId)
    {
        if (UserId != null)
            throw new InvalidOperationException("Cart is already assigned to a user");

        UserId = userId;
        SessionId = null;
        ExpiresAt = null; // User carts don't expire
        UpdateLastModified();
    }

    public int GetTotalItems() => _items.Sum(i => i.Quantity);

    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;

    public bool IsEmpty() => !_items.Any();

    private void UpdateLastModified()
    {
        LastModified = DateTime.UtcNow;
        UpdateTimestamp();
    }
}
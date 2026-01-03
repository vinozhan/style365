using Style365.Domain.Common;

namespace Style365.Domain.Entities;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsDefault { get; private set; }
    public bool IsPublic { get; private set; }

    public User User { get; private set; } = null!;
    private readonly List<WishlistItem> _items = [];
    public IReadOnlyList<WishlistItem> Items => _items.AsReadOnly();

    private Wishlist() { }

    public Wishlist(Guid userId, string name, bool isDefault = false)
    {
        UserId = userId;
        Name = ValidateName(name);
        IsDefault = isDefault;
        IsPublic = false;
    }

    public void UpdateName(string name)
    {
        Name = ValidateName(name);
        UpdateTimestamp();
    }

    public void SetAsDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdateTimestamp();
    }

    public void SetVisibility(bool isPublic)
    {
        IsPublic = isPublic;
        UpdateTimestamp();
    }

    public void AddProduct(Guid productId)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem == null)
        {
            var wishlistItem = new WishlistItem(Id, productId);
            _items.Add(wishlistItem);
            UpdateTimestamp();
        }
    }

    public void RemoveProduct(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            UpdateTimestamp();
        }
    }

    public bool ContainsProduct(Guid productId) => _items.Any(i => i.ProductId == productId);

    public int GetItemCount() => _items.Count;

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Wishlist name cannot be empty", nameof(name));
            
        name = name.Trim();
        
        if (name.Length < 2)
            throw new ArgumentException("Wishlist name must be at least 2 characters", nameof(name));
            
        return name;
    }
}
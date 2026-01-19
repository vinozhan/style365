using Style365.Domain.Common;

namespace Style365.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    public Guid? ParentCategoryId { get; private set; }

    public Category? ParentCategory { get; private set; }
    private readonly List<Category> _subCategories = [];
    private readonly List<Product> _products = [];

    public IReadOnlyList<Category> SubCategories => _subCategories.AsReadOnly();
    public IReadOnlyList<Product> Products => _products.AsReadOnly();

    private Category() {}
    
    public Category(string name, string slug, string? description = null, string? imageUrl = null, 
        bool isActive = true, int sortOrder = 0, Guid? parentCategoryId = null)
    {
        Name = ValidateName(name);
        Slug = slug;
        Description = description?.Trim();
        ImageUrl = imageUrl?.Trim();
        IsActive = isActive;
        SortOrder = sortOrder;
        ParentCategoryId = parentCategoryId;
    }

    public void Update(string name, string? description, string? imageUrl, bool isActive, int sortOrder, Guid? parentCategoryId)
    {
        Name = ValidateName(name);
        Description = description?.Trim();
        ImageUrl = imageUrl?.Trim();
        IsActive = isActive;
        SortOrder = sortOrder;
        ParentCategoryId = parentCategoryId;
        UpdateTimestamp();
    }

    public void UpdateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be empty", nameof(slug));
        
        Slug = slug.Trim().ToLowerInvariant();
        UpdateTimestamp();
    }

    public void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
            throw new ArgumentException("Sort order cannot be negative", nameof(sortOrder));
            
        SortOrder = sortOrder;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    public bool IsSubCategory() => ParentCategoryId.HasValue;

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));
            
        name = name.Trim();
        
        if (name.Length < 2)
            throw new ArgumentException("Category name must be at least 2 characters", nameof(name));
            
        return name;
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
                   .Replace(" ", "-")
                   .Replace("&", "and")
                   .ToCharArray()
                   .Where(c => char.IsLetterOrDigit(c) || c == '-')
                   .Aggregate("", (current, c) => current + c);
    }
}
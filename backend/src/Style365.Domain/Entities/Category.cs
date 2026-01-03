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
    
    public Category(string name, string? description = null, Guid? parentCategoryId = null)
    {
        Name = ValidateName(name);
        Slug = GenerateSlug(name);
        Description = description?.Trim();
        ParentCategoryId = parentCategoryId;
        IsActive = true;
        SortOrder = 0;
    }

    public void UpdateDetails(string name, string? description, string? imageUrl)
    {
        Name = ValidateName(name);
        Slug = GenerateSlug(name);
        Description = description?.Trim();
        ImageUrl = imageUrl?.Trim();
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
using Style365.Domain.Common;

namespace Style365.Domain.Entities;

public class ProductTag : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Product> _products = [];
    public IReadOnlyList<Product> Products => _products.AsReadOnly();

    private ProductTag() { }

    public ProductTag(string name, string? description = null)
    {
        Name = ValidateName(name);
        Slug = GenerateSlug(name);
        Description = description?.Trim();
        IsActive = true;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = ValidateName(name);
        Slug = GenerateSlug(name);
        Description = description?.Trim();
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

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty", nameof(name));
            
        name = name.Trim();
        
        if (name.Length < 2)
            throw new ArgumentException("Tag name must be at least 2 characters", nameof(name));
            
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
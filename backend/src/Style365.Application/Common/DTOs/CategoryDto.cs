namespace Style365.Application.Common.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? ParentCategoryName { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = [];
    public int ProductCount { get; set; }
}
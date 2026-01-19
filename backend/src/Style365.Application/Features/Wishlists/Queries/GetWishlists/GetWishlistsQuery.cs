using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Wishlists.Queries.GetWishlists;

public class GetWishlistsQuery : IRequest<Result<List<WishlistDto>>>
{
    public Guid UserId { get; set; }
}

public class WishlistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsPublic { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WishlistItemDto> Items { get; set; } = new();
}

public class WishlistItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string? ProductImage { get; set; }
    public bool IsInStock { get; set; }
    public DateTime AddedAt { get; set; }
}
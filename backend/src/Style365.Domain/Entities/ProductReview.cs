using Style365.Domain.Common;

namespace Style365.Domain.Entities;

public class ProductReview : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public int Rating { get; private set; }
    public string? Title { get; private set; }
    public string? Comment { get; private set; }
    public bool IsVerifiedPurchase { get; private set; }
    public bool IsApproved { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovedBy { get; private set; }

    public Product Product { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private ProductReview() { }

    public ProductReview(Guid productId, Guid userId, int rating, string? title = null, string? comment = null, bool isVerifiedPurchase = false)
    {
        ProductId = productId;
        UserId = userId;
        Rating = ValidateRating(rating);
        Title = title?.Trim();
        Comment = comment?.Trim();
        IsVerifiedPurchase = isVerifiedPurchase;
        IsApproved = false;
    }

    public void UpdateReview(int rating, string? title, string? comment)
    {
        Rating = ValidateRating(rating);
        Title = title?.Trim();
        Comment = comment?.Trim();
        
        // Reset approval when review is modified
        IsApproved = false;
        ApprovedAt = null;
        ApprovedBy = null;
        
        UpdateTimestamp();
    }

    public void Approve(string? approvedBy = null)
    {
        IsApproved = true;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy?.Trim();
        UpdateTimestamp();
    }

    public void Reject()
    {
        IsApproved = false;
        ApprovedAt = null;
        ApprovedBy = null;
        UpdateTimestamp();
    }

    private static int ValidateRating(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));
        return rating;
    }
}
using Style365.Domain.Common;
using Style365.Domain.Enums;

namespace Style365.Domain.Entities;

public class ProductReview : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? OrderItemId { get; private set; } // Link to verified purchase
    public int Rating { get; private set; }
    public string? Title { get; private set; }
    public string? Comment { get; private set; }
    public bool IsVerifiedPurchase { get; private set; }
    public ReviewStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public string? ModerationNotes { get; private set; }
    
    // Helpfulness tracking
    public int HelpfulCount { get; private set; }
    public int NotHelpfulCount { get; private set; }
    
    // Legacy fields for backward compatibility
    public bool IsApproved => Status == ReviewStatus.Published;
    public DateTime? ApprovedAt => PublishedAt;
    public string? ApprovedBy { get; private set; }

    public Product Product { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public OrderItem? OrderItem { get; private set; }

    private ProductReview() { }

    public ProductReview(Guid productId, Guid userId, int rating, string? title = null, string? comment = null, Guid? orderItemId = null, bool isNewUser = false)
    {
        ProductId = productId;
        UserId = userId;
        OrderItemId = orderItemId;
        Rating = ValidateRating(rating);
        Title = title?.Trim() ?? string.Empty;
        Comment = comment?.Trim() ?? string.Empty;
        IsVerifiedPurchase = orderItemId.HasValue;
        
        // Smart status determination with instant visibility for verified purchases
        Status = DetermineInitialStatus(orderItemId, Title, Comment, isNewUser);
        
        if (Status == ReviewStatus.Published)
        {
            PublishedAt = DateTime.UtcNow;
        }
    }

    public void UpdateReview(int rating, string? title, string? comment)
    {
        Rating = ValidateRating(rating);
        Title = title?.Trim() ?? string.Empty;
        Comment = comment?.Trim() ?? string.Empty;
        
        // Re-evaluate status after update
        Status = DetermineInitialStatus(OrderItemId, Title, Comment, false);
        if (Status == ReviewStatus.Published && PublishedAt == null)
        {
            PublishedAt = DateTime.UtcNow;
        }
        
        UpdateTimestamp();
    }

    public void Approve(string? moderatorNotes = null)
    {
        if (Status == ReviewStatus.Published)
            return; // Already approved

        Status = ReviewStatus.Published;
        PublishedAt = DateTime.UtcNow;
        ApprovedBy = moderatorNotes?.Trim();
        ModerationNotes = moderatorNotes?.Trim();
        UpdateTimestamp();
    }

    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required");

        Status = ReviewStatus.Rejected;
        ModerationNotes = reason.Trim();
        UpdateTimestamp();
    }

    public void Flag(string reason)
    {
        Status = ReviewStatus.Flagged;
        ModerationNotes = reason?.Trim();
        UpdateTimestamp();
    }

    public void MarkHelpful()
    {
        HelpfulCount++;
        UpdateTimestamp();
    }

    public void MarkNotHelpful()
    {
        NotHelpfulCount++;
        
        // Auto-flag if too many "not helpful" votes
        if (NotHelpfulCount >= 10 && GetHelpfulnessRatio() < 0.3)
        {
            Flag("Auto-flagged due to low helpfulness ratio");
        }
        
        UpdateTimestamp();
    }

    public double GetHelpfulnessRatio()
    {
        var total = HelpfulCount + NotHelpfulCount;
        return total == 0 ? 0 : (double)HelpfulCount / total;
    }

    public bool IsVisible() => Status == ReviewStatus.Published;

    private static ReviewStatus DetermineInitialStatus(Guid? orderItemId, string title, string comment, bool isNewUser)
    {
        // Auto-approve verified purchases with good content from existing users
        if (orderItemId.HasValue && PassesContentFilter(title, comment) && !isNewUser)
        {
            return ReviewStatus.Published;
        }
        
        // Queue for moderation in these cases:
        // 1. Unverified purchases
        // 2. New users (first review)  
        // 3. Content that doesn't pass filters
        return ReviewStatus.Pending;
    }

    private static bool PassesContentFilter(string title, string comment)
    {
        var combinedText = $"{title} {comment}".ToLowerInvariant();
        
        // Basic quality checks
        if (combinedText.Length < 5) return false; // Too short
        if (combinedText.Length > 2000) return false; // Too long
        
        // Check for suspicious patterns
        var suspiciousPatterns = new[]
        {
            "http://", "https://", "www.", ".com", ".org", // URLs
            "email me", "contact me", "call me", // Contact info
            "fake", "scam", "terrible", "worst ever" // Extreme language
        };

        var suspiciousCount = suspiciousPatterns.Count(pattern => combinedText.Contains(pattern));
        return suspiciousCount < 2; // Allow some flexibility
    }

    private static int ValidateRating(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));
        return rating;
    }
}
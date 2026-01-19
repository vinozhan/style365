namespace Style365.Domain.Enums;

public enum ReviewStatus
{
    Published = 0,      // Auto-approved, visible immediately  
    Pending = 1,        // Queued for manual review
    Rejected = 2,       // Hidden after review
    Flagged = 3         // User-reported, needs review
}
namespace Style365.Application.Features.Reviews.Queries.GetReviewStats;

public class GetReviewStatsResponse
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int VerifiedPurchaseReviews { get; set; }
    public RatingDistribution RatingDistribution { get; set; } = new();
}

public class RatingDistribution
{
    public int FiveStar { get; set; }
    public int FourStar { get; set; }
    public int ThreeStar { get; set; }
    public int TwoStar { get; set; }
    public int OneStar { get; set; }
    
    public Dictionary<int, double> GetPercentages(int totalReviews)
    {
        if (totalReviews == 0) return new Dictionary<int, double>();
        
        return new Dictionary<int, double>
        {
            { 5, Math.Round((double)FiveStar / totalReviews * 100, 1) },
            { 4, Math.Round((double)FourStar / totalReviews * 100, 1) },
            { 3, Math.Round((double)ThreeStar / totalReviews * 100, 1) },
            { 2, Math.Round((double)TwoStar / totalReviews * 100, 1) },
            { 1, Math.Round((double)OneStar / totalReviews * 100, 1) }
        };
    }
}
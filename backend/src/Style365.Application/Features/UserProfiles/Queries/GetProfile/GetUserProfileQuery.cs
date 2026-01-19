using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.UserProfiles.Queries.GetProfile;

public class GetUserProfileQuery : IRequest<Result<UserProfileDto>>
{
    public Guid UserId { get; set; }
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public string PreferredLanguage { get; set; } = string.Empty;
    public string PreferredCurrency { get; set; } = string.Empty;
    public List<string> FavoriteCategories { get; set; } = new();
    public List<string> PreferredBrands { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
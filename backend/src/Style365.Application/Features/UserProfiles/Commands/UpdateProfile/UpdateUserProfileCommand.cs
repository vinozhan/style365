using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.UserProfiles.Commands.UpdateProfile;

public class UpdateUserProfileCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public string PreferredLanguage { get; set; } = "en";
    public string PreferredCurrency { get; set; } = "USD";
    public List<string> FavoriteCategories { get; set; } = new();
    public List<string> PreferredBrands { get; set; } = new();
}
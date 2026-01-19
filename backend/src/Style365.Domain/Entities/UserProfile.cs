using Style365.Domain.Common;
using Style365.Domain.ValueObjects;

namespace Style365.Domain.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public bool EmailNotifications { get; private set; } = true;
    public bool SmsNotifications { get; private set; } = false;
    public string? PreferredLanguage { get; private set; } = "en";
    public string? PreferredCurrency { get; private set; } = "USD";

    // Addresses
    private readonly List<Address> _addresses = [];
    public IReadOnlyList<Address> Addresses => _addresses.AsReadOnly();

    // Preferences
    public string? FavoriteCategories { get; private set; } // JSON array
    public string? PreferredBrands { get; private set; } // JSON array

    // Navigation properties
    public User User { get; private set; } = null!;

    private UserProfile() { }

    public UserProfile(Guid userId, string firstName, string lastName)
    {
        UserId = userId;
        FirstName = firstName?.Trim() ?? throw new ArgumentException("First name cannot be empty");
        LastName = lastName?.Trim() ?? throw new ArgumentException("Last name cannot be empty");
    }

    public void UpdateBasicInfo(string firstName, string lastName, string? phoneNumber = null)
    {
        FirstName = firstName?.Trim() ?? throw new ArgumentException("First name cannot be empty");
        LastName = lastName?.Trim() ?? throw new ArgumentException("Last name cannot be empty");
        PhoneNumber = phoneNumber?.Trim();
        UpdateTimestamp();
    }

    public void UpdatePersonalInfo(DateTime? dateOfBirth, string? gender)
    {
        DateOfBirth = dateOfBirth;
        Gender = gender?.Trim();
        UpdateTimestamp();
    }

    public void UpdateProfileImage(string? imageUrl)
    {
        ProfileImageUrl = imageUrl?.Trim();
        UpdateTimestamp();
    }

    public void UpdateNotificationPreferences(bool emailNotifications, bool smsNotifications)
    {
        EmailNotifications = emailNotifications;
        SmsNotifications = smsNotifications;
        UpdateTimestamp();
    }

    public void UpdateLanguagePreferences(string language, string currency)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be empty");
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty");

        PreferredLanguage = language.Trim();
        PreferredCurrency = currency.Trim();
        UpdateTimestamp();
    }

    public void AddAddress(Address address)
    {
        if (address == null)
            throw new ArgumentNullException(nameof(address));

        _addresses.Add(address);
        UpdateTimestamp();
    }

    public void RemoveAddress(Address address)
    {
        if (address != null)
        {
            _addresses.Remove(address);
            UpdateTimestamp();
        }
    }

    public void UpdateFavoriteCategories(List<string> categoryNames)
    {
        FavoriteCategories = categoryNames?.Any() == true 
            ? System.Text.Json.JsonSerializer.Serialize(categoryNames) 
            : null;
        UpdateTimestamp();
    }

    public void UpdatePreferredBrands(List<string> brandNames)
    {
        PreferredBrands = brandNames?.Any() == true 
            ? System.Text.Json.JsonSerializer.Serialize(brandNames) 
            : null;
        UpdateTimestamp();
    }

    public List<string> GetFavoriteCategories()
    {
        return string.IsNullOrEmpty(FavoriteCategories) 
            ? new List<string>() 
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(FavoriteCategories) ?? new List<string>();
    }

    public List<string> GetPreferredBrands()
    {
        return string.IsNullOrEmpty(PreferredBrands) 
            ? new List<string>() 
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(PreferredBrands) ?? new List<string>();
    }

    public string GetFullName() => $"{FirstName} {LastName}".Trim();
}
using Style365.Domain.Common;
using Style365.Domain.Enums;
using Style365.Domain.ValueObjects;

namespace Style365.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsPhoneVerified { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? CognitoUserId { get; private set; }

    private readonly List<UserAddress> _addresses = [];
    private readonly List<Order> _orders = [];

    public IReadOnlyList<UserAddress> Addresses => _addresses.AsReadOnly();
    public IReadOnlyList<Order> Orders => _orders.AsReadOnly();

    private User() { }

    public User(string firstName, string lastName, string email, UserRole role = UserRole.Customer)
    {
        FirstName = ValidateName(firstName, nameof(firstName));
        LastName = ValidateName(lastName, nameof(lastName));
        Email = Email.Create(email);
        Role = role;
        IsActive = true;
        IsEmailVerified = false;
        IsPhoneVerified = false;
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber, DateTime? dateOfBirth)
    {
        FirstName = ValidateName(firstName, nameof(firstName));
        LastName = ValidateName(lastName, nameof(lastName));
        PhoneNumber = phoneNumber?.Trim();
        DateOfBirth = dateOfBirth;
        UpdateTimestamp();
    }

    public void MarkEmailVerified()
    {
        IsEmailVerified = true;
        UpdateTimestamp();
    }

    public void VerifyPhone()
    {
        if (PhoneNumber == null)
            throw new InvalidOperationException("Phone number is missing");

        IsPhoneVerified = true;
        UpdateTimestamp();
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void SetCognitoUserId(string cognitoUserId)
    {
        if (string.IsNullOrWhiteSpace(cognitoUserId))
            throw new ArgumentException("Cognito user ID cannot be empty", nameof(cognitoUserId));

        // Validate that Cognito sub is a valid GUID format
        if (!Guid.TryParse(cognitoUserId, out var cognitoGuid))
            throw new ArgumentException(
                $"Cognito user ID must be a valid GUID format. Received: '{cognitoUserId}'",
                nameof(cognitoUserId));

        if (CognitoUserId != null)
        {
            if (CognitoUserId == cognitoUserId)
                return; // idempotent retry

            throw new InvalidOperationException(
                "CognitoUserId already set and cannot be changed.");
        }

        // Use Cognito sub (UUID) as the User's primary key
        // This ensures JWT sub = User.Id = FK references across all entities
        Id = cognitoGuid;
        CognitoUserId = cognitoUserId;

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

    public void UpdateRole(UserRole role)
    {
        if (Role != role)
        {
            Role = role;
            UpdateTimestamp();
        }
    }

    public void AddAddress(Address address, bool isDefault = false)
    {
        var userAddress = new UserAddress(Id, address, isDefault);

        if (isDefault)
        {
            foreach (var existingAddress in _addresses)
                existingAddress.SetAsDefault(false);
        }

        _addresses.Add(userAddress);
        UpdateTimestamp();
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    private static string ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", paramName);

        name = name.Trim();

        if (name.Length < 2)
            throw new ArgumentException("Name must be at least 2 characters", paramName);

        return name;
    }
}
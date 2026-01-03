using Style365.Domain.Common;
using Style365.Domain.ValueObjects;

namespace Style365.Domain.Entities;

public class UserAddress : BaseEntity
{
    public Guid UserId { get; private set; }
    public Address Address { get; private set; } = null!;
    public bool IsDefault { get; private set; }
    public string? Label { get; private set; }

    public User User { get; private set; } = null!;

    private UserAddress() { }

    public UserAddress(Guid userId, Address address, bool isDefault = false, string? label = null)
    {
        UserId = userId;
        Address = address;
        IsDefault = isDefault;
        Label = label?.Trim();
    }

    public void SetAsDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdateTimestamp();
    }

    public void UpdateAddress(Address address)
    {
        Address = address;
        UpdateTimestamp();
    }

    public void UpdateLabel(string? label)
    {
        Label = label?.Trim();
        UpdateTimestamp();
    }
}
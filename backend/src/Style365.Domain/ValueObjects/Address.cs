namespace Style365.Domain.ValueObjects;

public record Address
{
    public string AddressLine1 { get; }
    public string? AddressLine2 { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address(string addressLine1, string? addressLine2, string city, 
                   string state, string postalCode, string country)
    {
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string addressLine1, string? addressLine2, 
                               string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(addressLine1))
            throw new ArgumentException("Address line 1 cannot be empty", nameof(addressLine1));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty", nameof(state));
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        return new Address(addressLine1.Trim(), addressLine2?.Trim(), 
                         city.Trim(), state.Trim(), postalCode.Trim(), country.Trim());
    }

    public string GetFullAddress()
    {
        var parts = new List<string> { AddressLine1 };
        
        if (!string.IsNullOrWhiteSpace(AddressLine2))
            parts.Add(AddressLine2);
            
        parts.AddRange(new[] { City, State, PostalCode, Country });
        
        return string.Join(", ", parts);
    }
}
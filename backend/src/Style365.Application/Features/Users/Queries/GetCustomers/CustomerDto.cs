namespace Style365.Application.Features.Users.Queries.GetCustomers;

public record CustomerDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public bool IsActive { get; init; }
    public bool IsEmailVerified { get; init; }
    public int OrdersCount { get; init; }
    public decimal TotalSpent { get; init; }
    public DateTime? LastOrderDate { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }

    public string FullName => $"{FirstName} {LastName}";
}

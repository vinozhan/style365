using Style365.Application.Common.DTOs;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand : ICommand<Result<UserDto>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public UserRole Role { get; init; } = UserRole.Customer;
    public string? CognitoUserId { get; init; }
}
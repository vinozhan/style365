using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Authentication.Commands.Login;

public record LoginCommand : ICommand<Result<AuthenticationResult>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Authentication.Commands.Login;

public class LoginHandler : ICommandHandler<LoginCommand, Result<AuthenticationResult>>
{
    private readonly IAuthenticationService _authenticationService;

    public LoginHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<Result<AuthenticationResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authenticationService.LoginAsync(request.Email, request.Password);
    }
}
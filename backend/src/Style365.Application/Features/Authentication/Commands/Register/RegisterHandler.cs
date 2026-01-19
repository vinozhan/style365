using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Authentication.Commands.Register;

public class RegisterHandler : ICommandHandler<RegisterCommand, Result<AuthenticationResult>>
{
    private readonly IAuthenticationService _authenticationService;

    public RegisterHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<Result<AuthenticationResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _authenticationService.RegisterAsync(
            request.Email, 
            request.Password, 
            request.FirstName, 
            request.LastName);
    }
}
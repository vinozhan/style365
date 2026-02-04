using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
}

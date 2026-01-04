using Style365.Application.Common.DTOs;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Users.Queries.GetUser;

public record GetUserQuery : IQuery<Result<UserDto>>
{
    public Guid UserId { get; init; }
}
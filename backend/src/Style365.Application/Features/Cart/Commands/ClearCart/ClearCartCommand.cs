using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Cart.Commands.ClearCart;

public class ClearCartCommand : IRequest<Result>
{
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
}
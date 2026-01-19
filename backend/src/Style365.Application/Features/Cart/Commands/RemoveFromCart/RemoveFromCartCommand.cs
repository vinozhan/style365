using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Cart.Commands.RemoveFromCart;

public class RemoveFromCartCommand : IRequest<Result>
{
    public Guid CartItemId { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
}
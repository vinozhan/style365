using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Cart.Commands.UpdateCartItem;

public class UpdateCartItemCommand : IRequest<Result>
{
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
}
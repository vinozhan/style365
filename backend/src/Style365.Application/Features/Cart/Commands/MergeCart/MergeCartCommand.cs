using MediatR;
using Style365.Application.Common.Models;
using Style365.Application.Features.Cart.Queries.GetCart;

namespace Style365.Application.Features.Cart.Commands.MergeCart;

public class MergeCartCommand : IRequest<Result<CartDto>>
{
    public Guid UserId { get; set; }
    public string SessionId { get; set; } = string.Empty;
}
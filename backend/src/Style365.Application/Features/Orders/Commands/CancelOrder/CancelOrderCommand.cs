using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommand : IRequest<Result>
{
    public Guid OrderId { get; set; }
    public Guid? UserId { get; set; } // For authorization
    public string Reason { get; set; } = string.Empty;
}
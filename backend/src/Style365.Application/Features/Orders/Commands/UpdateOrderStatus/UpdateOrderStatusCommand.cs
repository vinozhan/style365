using MediatR;
using Style365.Application.Common.Models;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommand : IRequest<Result>
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ShippingCarrier { get; set; }
    public string? Notes { get; set; }
}
using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Orders.Commands.UpdateTracking;

public class UpdateTrackingCommand : IRequest<Result>
{
    public Guid OrderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string ShippingCarrier { get; set; } = string.Empty;
    public DateTime? ShippedDate { get; set; }
}
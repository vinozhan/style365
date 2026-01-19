using MediatR;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Orders.Commands.UpdateTracking;

public class UpdateTrackingHandler : IRequestHandler<UpdateTrackingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTrackingHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTrackingCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId);
        }

        try
        {
            // Update tracking information - this would typically happen when order is shipped
            if (request.ShippedDate.HasValue)
            {
                // Ship the order with tracking details
                order.Ship(request.TrackingNumber, request.ShippingCarrier);
            }
            else
            {
                // Just update tracking info without changing status
                // This might require adding a method to the Order entity
                // For now, we'll only allow updating when shipping
                return Result.Failure("Tracking information can only be added when order is shipped");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
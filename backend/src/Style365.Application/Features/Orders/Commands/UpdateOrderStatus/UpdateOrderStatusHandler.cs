using MediatR;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId);
        }

        try
        {
            // Update order status based on the requested status
            switch (request.Status)
            {
                case OrderStatus.Confirmed:
                    order.Confirm();
                    break;
                
                case OrderStatus.Processing:
                    order.StartProcessing();
                    break;
                
                case OrderStatus.Shipped:
                    order.Ship(request.TrackingNumber, request.ShippingCarrier);
                    break;
                
                case OrderStatus.OutForDelivery:
                    order.MarkOutForDelivery();
                    break;
                
                case OrderStatus.Delivered:
                    order.Deliver();
                    break;
                
                case OrderStatus.Cancelled:
                    return Result.Failure("Use CancelOrder command to cancel orders");
                
                default:
                    return Result.Failure($"Invalid status transition to {request.Status}");
            }

            // Add notes if provided
            if (!string.IsNullOrEmpty(request.Notes))
            {
                order.AddNotes(request.Notes);
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
using MediatR;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId);
        }

        // Check authorization - users can only cancel their own orders
        if (request.UserId.HasValue && order.UserId != request.UserId)
        {
            return Result.Failure("Unauthorized to cancel this order");
        }

        // Check if order can be cancelled
        if (order.Status == OrderStatus.Cancelled)
        {
            return Result.Failure("Order is already cancelled");
        }

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
        {
            return Result.Failure("Cannot cancel shipped or delivered orders");
        }

        // Cancel the order
        order.Cancel(request.Reason);

        // Restore inventory
        await RestoreInventory(order, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task RestoreInventory(Order order, CancellationToken cancellationToken)
    {
        foreach (var orderItem in order.Items)
        {
            var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(orderItem.ProductId, cancellationToken);
            if (product != null && product.TrackQuantity)
            {
                if (orderItem.ProductVariantId.HasValue)
                {
                    var variant = product.Variants.FirstOrDefault(v => v.Id == orderItem.ProductVariantId);
                    if (variant != null)
                    {
                        variant.UpdateInventory(variant.StockQuantity + orderItem.Quantity);
                    }
                }
                else
                {
                    product.IncreaseStock(orderItem.Quantity);
                }
            }
        }
    }
}
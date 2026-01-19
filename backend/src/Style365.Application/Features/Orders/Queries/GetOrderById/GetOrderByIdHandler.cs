using MediatR;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrderByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        Order? order = null;
        
        // Find order by ID or order number
        if (request.OrderId.HasValue)
        {
            order = await _unitOfWork.Orders.GetByIdWithFullDetailsAsync(request.OrderId.Value, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.OrderNumber))
        {
            order = await _unitOfWork.Orders.GetByOrderNumberAsync(request.OrderNumber, cancellationToken);
            if (order != null)
            {
                order = await _unitOfWork.Orders.GetByIdWithFullDetailsAsync(order.Id, cancellationToken);
            }
        }
        
        if (order == null)
        {
            return Result.Failure<OrderDto>("Order not found");
        }

        // Check authorization
        if (request.UserId.HasValue && order.UserId != request.UserId)
        {
            return Result.Failure<OrderDto>("Unauthorized to view this order");
        }
        
        // For order tracking, verify email matches
        if (!string.IsNullOrEmpty(request.CustomerEmail) && 
            !string.Equals(order.CustomerEmail, request.CustomerEmail, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<OrderDto>("Order not found");
        }

        var orderDto = new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            Status = order.Status,
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            CreatedAt = order.CreatedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            TrackingNumber = order.TrackingNumber,
            ShippingCarrier = order.ShippingCarrier,
            Notes = order.Notes,
            ShippingAddress = new OrderAddressDto
            {
                AddressLine1 = order.ShippingAddress.AddressLine1,
                City = order.ShippingAddress.City,
                StateProvince = order.ShippingAddress.State,
                PostalCode = order.ShippingAddress.PostalCode,
                Country = order.ShippingAddress.Country
            },
            BillingAddress = new OrderAddressDto
            {
                AddressLine1 = order.BillingAddress.AddressLine1,
                City = order.BillingAddress.City,
                StateProvince = order.BillingAddress.State,
                PostalCode = order.BillingAddress.PostalCode,
                Country = order.BillingAddress.Country
            }
        };

        // Map order items
        foreach (var item in order.Items)
        {
            var itemDto = new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductSku = item.ProductSku,
                ProductImage = item.ProductImage,
                VariantId = item.ProductVariantId,
                VariantName = item.VariantName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice.Amount,
                LineTotal = item.GetLineTotal().Amount,
                Currency = item.UnitPrice.Currency
            };
            orderDto.Items.Add(itemDto);
        }

        // Map payments
        foreach (var payment in order.Payments)
        {
            var paymentDto = new OrderPaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount.Amount,
                Currency = payment.Amount.Currency,
                Method = payment.Method,
                Status = payment.Status,
                Reference = payment.PaymentReference,
                CreatedAt = payment.CreatedAt
            };
            orderDto.Payments.Add(paymentDto);
        }

        return Result.Success(orderDto);
    }
}
using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using Style365.Domain.Enums;
using Style365.Domain.ValueObjects;
using ShoppingCart = Style365.Domain.Entities.ShoppingCart;


namespace Style365.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Get the shopping cart
        Style365.Domain.Entities.ShoppingCart? cart = null;
        if (request.UserId.HasValue)
        {
            cart = await _unitOfWork.ShoppingCarts.GetByUserIdWithItemsAsync(request.UserId.Value, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.SessionId))
        {
            cart = await _unitOfWork.ShoppingCarts.GetBySessionIdWithItemsAsync(request.SessionId, cancellationToken);
        }

        if (cart == null || cart.IsEmpty())
        {
            return Result.Failure<CreateOrderResponse>("Shopping cart is empty or not found");
        }

        // Verify all items are still available and in stock
        foreach (var cartItem in cart.Items)
        {
            var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(cartItem.ProductId, cancellationToken);
            if (product == null || !product.IsActive)
            {
                return Result.Failure<CreateOrderResponse>($"Product {cartItem.ProductId} is no longer available");
            }

            // Check stock
            var availableStock = cartItem.ProductVariantId.HasValue
                ? product.Variants.FirstOrDefault(v => v.Id == cartItem.ProductVariantId)?.StockQuantity ?? 0
                : product.StockQuantity;

            if (product.TrackQuantity && availableStock < cartItem.Quantity)
            {
                return Result.Failure<CreateOrderResponse>($"Insufficient stock for {product.Name}. Available: {availableStock}, Required: {cartItem.Quantity}");
            }
        }

        // Create shipping address
        var shippingAddress = Address.Create(
            request.ShippingAddress.AddressLine1,
            request.ShippingAddress.AddressLine2,
            request.ShippingAddress.City,
            request.ShippingAddress.StateProvince,
            request.ShippingAddress.PostalCode,
            request.ShippingAddress.Country
        );

        // Create billing address (use shipping if not provided)
        Address billingAddress;
        if (request.BillingAddress != null)
        {
            billingAddress = Address.Create(
                request.BillingAddress.AddressLine1,
                request.BillingAddress.AddressLine2,
                request.BillingAddress.City,
                request.BillingAddress.StateProvince,
                request.BillingAddress.PostalCode,
                request.BillingAddress.Country
            );
        }
        else
        {
            billingAddress = shippingAddress;
        }

        // Generate order number
        var orderNumber = await GenerateOrderNumber();

        // Calculate total
        var cartTotal = cart.GetTotal();

        // Create order
        var order = new Order(
            orderNumber,
            request.UserId,
            cartTotal,
            shippingAddress,
            billingAddress,
            request.CustomerEmail,
            request.CustomerPhone
        );

        // Add order items
        foreach (var cartItem in cart.Items)
        {
            var orderItem = new OrderItem(
                cartItem.ProductId,
                cartItem.ProductVariantId,
                cartItem.Quantity,
                cartItem.UnitPrice
            );
            order.AddItem(orderItem);
        }

        // Set order notes if provided
        if (!string.IsNullOrWhiteSpace(request.OrderNotes))
        {
            order.AddNotes(request.OrderNotes);
        }

        // Create payment record if payment method is not Cash on Delivery
        if (request.PaymentMethod != PaymentMethod.CashOnDelivery)
        {
            var payment = new Payment(
                order.Id,
                cartTotal,
                request.PaymentMethod
            );

            if (!string.IsNullOrWhiteSpace(request.PaymentReference))
            {
                payment.SetExternalReference(request.PaymentReference);
            }

            await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
        }
        else
        {
            // For COD orders, set status to confirmed
            order.Confirm();
        }

        // Reserve inventory
        await ReserveInventory(cart, cancellationToken);

        // Save order
        await _unitOfWork.Orders.AddAsync(order, cancellationToken);

        // Clear the cart
        cart.ClearCart();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateOrderResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };

        return Result.Success(response);
    }

    private async Task<string> GenerateOrderNumber()
    {
        // Generate order number like ORD-2024-000001
        var year = DateTime.UtcNow.Year;
        var lastOrder = await _unitOfWork.Orders.GetLastOrderByYearAsync(year);

        int nextNumber = 1;
        if (lastOrder != null)
        {
            // Extract number from last order and increment
            var lastOrderNumber = lastOrder.OrderNumber;
            if (lastOrderNumber.Contains($"ORD-{year}-"))
            {
                var numberPart = lastOrderNumber.Split('-').Last();
                if (int.TryParse(numberPart, out var number))
                {
                    nextNumber = number + 1;
                }
            }
        }

        return $"ORD-{year}-{nextNumber:D6}";
    }

    private async Task ReserveInventory(ShoppingCart cart, CancellationToken cancellationToken)
    {
        foreach (var cartItem in cart.Items)
        {
            var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(cartItem.ProductId, cancellationToken);
            if (product != null && product.TrackQuantity)
            {
                if (cartItem.ProductVariantId.HasValue)
                {
                    var variant = product.Variants.FirstOrDefault(v => v.Id == cartItem.ProductVariantId);
                    if (variant != null)
                    {
                        variant.UpdateInventory(variant.StockQuantity - cartItem.Quantity);
                    }
                }
                else
                {
                    product.ReduceStock(cartItem.Quantity);
                }
            }
        }
    }
}
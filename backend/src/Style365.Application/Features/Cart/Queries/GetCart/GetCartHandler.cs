using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Cart.Queries.GetCart;

public class GetCartHandler : IRequestHandler<GetCartQuery, Result<CartDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCartHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = null;

        if (request.UserId.HasValue)
        {
            cart = await _unitOfWork.ShoppingCarts.GetByUserIdWithItemsAsync(request.UserId.Value, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.SessionId))
        {
            cart = await _unitOfWork.ShoppingCarts.GetBySessionIdWithItemsAsync(request.SessionId, cancellationToken);
        }
        else
        {
            return Result.Failure<CartDto>("Either UserId or SessionId must be provided");
        }

        if (cart == null)
        {
            // Return empty cart
            return Result.Success(new CartDto
            {
                UserId = request.UserId,
                SessionId = request.SessionId,
                Items = new List<CartItemDto>(),
                TotalItems = 0,
                SubTotal = 0,
                TotalAmount = 0
            });
        }

        // Check if cart is expired
        if (cart.IsExpired())
        {
            return Result.Failure<CartDto>("Cart has expired");
        }

        var cartDto = await MapToCartDto(cart, cancellationToken);
        return Result.Success(cartDto);
    }

    private async Task<CartDto> MapToCartDto(Style365.Domain.Entities.ShoppingCart cart, CancellationToken cancellationToken)
    {
        var itemDtos = new List<CartItemDto>();

        foreach (var item in cart.Items)
        {
            var product = await _unitOfWork.Products.GetByIdWithImagesAsync(item.ProductId, cancellationToken);
            ProductVariant? variant = null;

            if (item.ProductVariantId.HasValue)
            {
                var productWithVariants = await _unitOfWork.Products.GetByIdWithVariantsAsync(item.ProductId, cancellationToken);
                variant = productWithVariants?.Variants.FirstOrDefault(v => v.Id == item.ProductVariantId);
            }

            var itemDto = new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = product?.Name ?? "Unknown Product",
                ProductSlug = product?.Slug ?? "",
                ProductImage = product?.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                VariantId = item.ProductVariantId,
                VariantName = variant?.Name,
                VariantColor = variant?.Color,
                VariantSize = variant?.Size,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice.Amount,
                SubTotal = item.GetSubtotal().Amount,
                Currency = item.UnitPrice.Currency,
                AddedAt = item.AddedAt
            };

            itemDtos.Add(itemDto);
        }

        var total = cart.GetTotal();

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            SessionId = cart.SessionId,
            Items = itemDtos,
            TotalItems = cart.GetTotalItems(),
            SubTotal = total.Amount,
            TotalAmount = total.Amount, // For now, same as subtotal (no taxes/shipping)
            Currency = total.Currency,
            LastModified = cart.LastModified,
            ExpiresAt = cart.ExpiresAt
        };
    }
}
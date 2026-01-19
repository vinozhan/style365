using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Application.Features.Cart.Queries.GetCart;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Cart.Commands.MergeCart;

public class MergeCartHandler : IRequestHandler<MergeCartCommand, Result<CartDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public MergeCartHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartDto>> Handle(MergeCartCommand request, CancellationToken cancellationToken)
    {
        // Get user's existing cart
        var userCart = await _unitOfWork.ShoppingCarts.GetByUserIdWithItemsAsync(request.UserId, cancellationToken);
        
        // Get guest cart
        var guestCart = await _unitOfWork.ShoppingCarts.GetBySessionIdWithItemsAsync(request.SessionId, cancellationToken);
        
        if (guestCart == null || !guestCart.Items.Any())
        {
            // No guest cart to merge, return user cart or create new one
            if (userCart == null)
            {
                userCart = new ShoppingCart(request.UserId);
                await _unitOfWork.ShoppingCarts.AddAsync(userCart, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            
            return Result.Success(MapToCartDto(userCart));
        }
        
        if (userCart == null)
        {
            // User has no cart, convert guest cart to user cart
            guestCart.AssignToUser(request.UserId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(MapToCartDto(guestCart));
        }
        
        // Merge guest cart items into user cart
        foreach (var guestItem in guestCart.Items.ToList())
        {
            var existingItem = userCart.Items.FirstOrDefault(ui => 
                ui.ProductId == guestItem.ProductId && 
                ui.ProductVariantId == guestItem.ProductVariantId);
            
            if (existingItem != null)
            {
                // Update quantity of existing item
                var newQuantity = existingItem.Quantity + guestItem.Quantity;
                
                // Check stock availability
                var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(guestItem.ProductId, cancellationToken);
                if (product != null)
                {
                    var availableStock = guestItem.ProductVariantId.HasValue
                        ? product.Variants.FirstOrDefault(v => v.Id == guestItem.ProductVariantId)?.StockQuantity ?? 0
                        : product.StockQuantity;
                    
                    if (product.TrackQuantity && availableStock < newQuantity)
                    {
                        newQuantity = Math.Min(newQuantity, availableStock);
                    }
                }
                
                existingItem.UpdateQuantity(newQuantity);
            }
            else
            {
                // Add new item to user cart
                userCart.AddItem(new CartItem(
                    guestItem.ProductId,
                    guestItem.ProductVariantId,
                    guestItem.Quantity,
                    guestItem.UnitPrice
                ));
            }
        }
        
        // Remove guest cart
        _unitOfWork.ShoppingCarts.Remove(guestCart);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success(MapToCartDto(userCart));
    }
    
    private static CartDto MapToCartDto(ShoppingCart cart)
    {
        var total = cart.GetTotal();
        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            SessionId = cart.SessionId,
            LastModified = cart.LastModified,
            ExpiresAt = cart.ExpiresAt,
            TotalAmount = total.Amount,
            SubTotal = total.Amount, // For simplicity, assuming no taxes/shipping in merge
            Currency = total.Currency,
            TotalItems = cart.Items.Sum(i => i.Quantity),
            Items = cart.Items.Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                VariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice.Amount,
                SubTotal = item.GetSubtotal().Amount,
                Currency = item.UnitPrice.Currency,
                AddedAt = item.AddedAt,
                // Product details would normally be populated by a proper mapping service
                ProductName = "", // Will be empty - controller should handle proper mapping
                ProductSlug = ""
            }).ToList()
        };
    }
}
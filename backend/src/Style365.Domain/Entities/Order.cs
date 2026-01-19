using Style365.Domain.Common;
using Style365.Domain.Enums;
using Style365.Domain.ValueObjects;

namespace Style365.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; private set; } = null!;
    public Guid? UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; } = null!;
    public Money Subtotal { get; private set; } = null!;
    public Money TaxAmount { get; private set; } = null!;
    public Money ShippingAmount { get; private set; } = null!;
    public Money DiscountAmount { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;
    public Address BillingAddress { get; private set; } = null!;
    public string CustomerEmail { get; private set; } = null!;
    public string? CustomerPhone { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? TrackingNumber { get; private set; }
    public string? ShippingCarrier { get; private set; }

    public User User { get; private set; } = null!;
    private readonly List<OrderItem> _items = [];
    private readonly List<Payment> _payments = [];

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyList<Payment> Payments => _payments.AsReadOnly();

    private Order(){}

    public Order(string orderNumber, Guid? userId, Money totalAmount, Address shippingAddress, Address billingAddress, string customerEmail, string? customerPhone = null)
    {
        OrderNumber = orderNumber;
        UserId = userId;
        TotalAmount = totalAmount;
        Status = OrderStatus.Pending;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        CreatedAt = DateTime.UtcNow;
        // Create separate Money instances - EF owned entities cannot be shared between properties
        Subtotal = Money.Create(totalAmount.Amount, totalAmount.Currency);
        TaxAmount = Money.Create(0, totalAmount.Currency);
        ShippingAmount = Money.Create(0, totalAmount.Currency);
        DiscountAmount = Money.Create(0, totalAmount.Currency);
    }

    public void AddItem(OrderItem orderItem)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed order");

        _items.Add(orderItem);
        UpdateTimestamp();
    }

    public void AddItem(Product product, int quantity, Money unitPrice, ProductVariant? variant = null)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed order");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id && 
                                               i.ProductVariantId == variant?.Id);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var orderItem = new OrderItem(Id, product.Id, quantity, unitPrice, variant?.Id);
            _items.Add(orderItem);
        }

        UpdateTimestamp();
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        Status = OrderStatus.Confirmed;
        UpdateTimestamp();
    }

    public void AddNotes(string notes)
    {
        Notes = notes?.Trim();
        UpdateTimestamp();
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel shipped or delivered orders");

        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled");

        Status = OrderStatus.Cancelled;
        Notes = string.IsNullOrEmpty(Notes) ? $"Cancelled: {reason}" : $"{Notes}\nCancelled: {reason}";
        UpdateTimestamp();
    }

    public void RemoveItem(Guid productId, Guid? variantId = null)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed order");

        var item = _items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == variantId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotals();
            UpdateTimestamp();
        }
    }

    public void UpdateItemQuantity(Guid productId, int newQuantity, Guid? variantId = null)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed order");

        var item = _items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == variantId);
        if (item != null)
        {
            if (newQuantity <= 0)
            {
                RemoveItem(productId, variantId);
            }
            else
            {
                item.UpdateQuantity(newQuantity);
                RecalculateTotals();
                UpdateTimestamp();
            }
        }
    }

    public void UpdateShipping(Money shippingAmount, string? carrier = null, string? trackingNumber = null)
    {
        ShippingAmount = shippingAmount;
        ShippingCarrier = carrier?.Trim();
        TrackingNumber = trackingNumber?.Trim();
        RecalculateTotals();
        UpdateTimestamp();
    }

    public void ApplyDiscount(Money discountAmount)
    {
        if (discountAmount.Amount < 0)
            throw new ArgumentException("Discount amount cannot be negative");

        DiscountAmount = discountAmount;
        RecalculateTotals();
        UpdateTimestamp();
    }

    public void UpdateTax(Money taxAmount)
    {
        TaxAmount = taxAmount;
        RecalculateTotals();
        UpdateTimestamp();
    }

    public void StartProcessing()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be processed");

        Status = OrderStatus.Processing;
        UpdateTimestamp();
    }

    public void Ship(string? trackingNumber = null, string? carrier = null)
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be shipped");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        TrackingNumber = trackingNumber?.Trim();
        ShippingCarrier = carrier?.Trim();
        UpdateTimestamp();
    }

    public void MarkOutForDelivery()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be out for delivery");

        Status = OrderStatus.OutForDelivery;
        UpdateTimestamp();
    }

    public void Deliver()
    {
        if (Status != OrderStatus.OutForDelivery)
            throw new InvalidOperationException("Only orders out for delivery can be delivered");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdateTimestamp();
    }


    public int GetTotalItems() => _items.Sum(i => i.Quantity);

    private void RecalculateTotals()
    {
        Subtotal = _items.Aggregate(Money.Zero(), (total, item) => total + item.GetLineTotal());
        TotalAmount = Subtotal + TaxAmount + ShippingAmount - DiscountAmount;
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}
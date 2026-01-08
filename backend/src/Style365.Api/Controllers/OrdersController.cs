using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Features.Orders.Commands.CreateOrder;
using Style365.Application.Features.Orders.Commands.CancelOrder;
using Style365.Application.Features.Orders.Commands.UpdateOrderStatus;
using Style365.Application.Features.Orders.Commands.UpdateTracking;
using Style365.Application.Features.Orders.Queries.GetOrderById;
using Style365.Application.Features.Orders.Queries.GetOrders;
using Style365.Domain.Enums;
using System.Security.Claims;

namespace Style365.Api.Controllers;

/// <summary>
/// Order management and checkout endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new order from cart contents
    /// </summary>
    /// <param name="command">Order details including shipping, billing, and payment info</param>
    /// <returns>Created order details</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid order data or insufficient stock</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        // Set user ID if authenticated
        command.UserId = GetCurrentUserId();
        
        if (!command.UserId.HasValue && string.IsNullOrWhiteSpace(command.SessionId))
        {
            return BadRequest(new { error = "Either authentication or session ID is required" });
        }

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetOrder), new { id = result.Data!.OrderId }, result.Data);
    }

    /// <summary>
    /// Get order details by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    /// <response code="200">Order found</response>
    /// <response code="404">Order not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var userId = GetCurrentUserId();
        var query = new GetOrderByIdQuery { OrderId = id, UserId = userId };
        
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Get current user's order history
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>User's orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var query = new GetOrdersQuery 
        { 
            UserId = userId, 
            Page = page, 
            PageSize = pageSize 
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Get order by order number (for order tracking)
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <param name="email">Customer email for verification</param>
    /// <returns>Order details</returns>
    /// <response code="200">Order found</response>
    /// <response code="404">Order not found or invalid email</response>
    [HttpGet("track/{orderNumber}")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> TrackOrder(string orderNumber, [FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { error = "Email is required for order tracking" });
        }

        var query = new GetOrderByIdQuery 
        { 
            OrderNumber = orderNumber,
            CustomerEmail = email 
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Cancel an order (only if not shipped)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Cancellation request with reason</param>
    /// <returns>No content</returns>
    /// <response code="204">Order cancelled successfully</response>
    /// <response code="400">Order cannot be cancelled</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Order not found</response>
    [HttpPost("{id}/cancel")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var command = new CancelOrderCommand 
        { 
            OrderId = id, 
            UserId = userId, 
            Reason = request.Reason 
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return NoContent();
    }

    /// <summary>
    /// Update order status (Admin only)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Status update details</param>
    /// <returns>No content</returns>
    /// <response code="204">Order status updated</response>
    /// <response code="400">Invalid status transition</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Admin access required</response>
    /// <response code="404">Order not found</response>
    [HttpPut("{id}/status")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var orderStatus))
        {
            return BadRequest(new { error = "Invalid order status" });
        }

        var command = new UpdateOrderStatusCommand 
        { 
            OrderId = id, 
            Status = orderStatus,
            Notes = request.Notes
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return NoContent();
    }

    /// <summary>
    /// Add tracking information to order (Admin only)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Tracking details</param>
    /// <returns>No content</returns>
    /// <response code="204">Tracking info added</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Admin access required</response>
    /// <response code="404">Order not found</response>
    [HttpPut("{id}/tracking")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateTracking(Guid id, [FromBody] UpdateTrackingRequest request)
    {
        var command = new UpdateTrackingCommand
        {
            OrderId = id,
            TrackingNumber = request.TrackingNumber,
            ShippingCarrier = request.ShippingCarrier,
            ShippedDate = request.ShippedDate
        };

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public class CancelOrderRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateTrackingRequest
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string ShippingCarrier { get; set; } = string.Empty;
    public DateTime? ShippedDate { get; set; }
}
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Features.Cart.Commands.AddToCart;
using Style365.Application.Features.Cart.Commands.RemoveFromCart;
using Style365.Application.Features.Cart.Commands.UpdateCartItem;
using Style365.Application.Features.Cart.Commands.ClearCart;
using Style365.Application.Features.Cart.Commands.MergeCart;
using Style365.Application.Features.Cart.Queries.GetCart;
using System.Security.Claims;

namespace Style365.Api.Controllers;

/// <summary>
/// Shopping cart management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current user's shopping cart
    /// </summary>
    /// <param name="sessionId">Session ID for guest cart (optional if authenticated)</param>
    /// <returns>Shopping cart contents</returns>
    /// <response code="200">Cart retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetCart([FromQuery] string? sessionId = null)
    {
        var userId = GetCurrentUserId();
        
        if (!userId.HasValue && string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(new { error = "Either authentication or session ID is required" });
        }

        var query = new GetCartQuery
        {
            UserId = userId,
            SessionId = sessionId
        };

        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Add item to shopping cart
    /// </summary>
    /// <param name="command">Item details to add</param>
    /// <returns>Updated cart summary</returns>
    /// <response code="200">Item added successfully</response>
    /// <response code="400">Invalid item or insufficient stock</response>
    [HttpPost("items")]
    [ProducesResponseType(typeof(AddToCartResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartCommand command)
    {
        // Set user ID if authenticated, otherwise use session ID from command
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

        return Ok(result.Data);
    }

    /// <summary>
    /// Update item quantity in cart
    /// </summary>
    /// <param name="itemId">Cart item ID</param>
    /// <param name="request">New quantity</param>
    /// <returns>No content</returns>
    /// <response code="204">Item updated successfully</response>
    /// <response code="400">Invalid quantity or item not found</response>
    /// <response code="404">Cart item not found</response>
    [HttpPut("items/{itemId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateCartItem(Guid itemId, [FromBody] UpdateCartItemRequest request)
    {
        var command = new UpdateCartItemCommand
        {
            CartItemId = itemId,
            Quantity = request.Quantity,
            UserId = GetCurrentUserId(),
            SessionId = Request.Headers["X-Session-Id"].FirstOrDefault()
        };

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return NoContent();
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    /// <param name="itemId">Cart item ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Item removed successfully</response>
    /// <response code="404">Cart item not found</response>
    [HttpDelete("items/{itemId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveCartItem(Guid itemId)
    {
        var command = new RemoveFromCartCommand
        {
            CartItemId = itemId,
            UserId = GetCurrentUserId(),
            SessionId = Request.Headers["X-Session-Id"].FirstOrDefault()
        };

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return NoContent();
    }

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    /// <param name="sessionId">Session ID for guest cart (optional if authenticated)</param>
    /// <returns>No content</returns>
    /// <response code="204">Cart cleared successfully</response>
    /// <response code="400">Invalid request parameters</response>
    [HttpDelete]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ClearCart([FromQuery] string? sessionId = null)
    {
        var userId = GetCurrentUserId();
        
        if (!userId.HasValue && string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(new { error = "Either authentication or session ID is required" });
        }

        var command = new ClearCartCommand
        {
            UserId = userId,
            SessionId = sessionId
        };

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return NoContent();
    }

    /// <summary>
    /// Get cart summary (item count and total)
    /// </summary>
    /// <param name="sessionId">Session ID for guest cart (optional if authenticated)</param>
    /// <returns>Cart summary</returns>
    /// <response code="200">Cart summary retrieved</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(CartSummaryResponse), 200)]
    public async Task<IActionResult> GetCartSummary([FromQuery] string? sessionId = null)
    {
        var userId = GetCurrentUserId();
        
        if (!userId.HasValue && string.IsNullOrWhiteSpace(sessionId))
        {
            return Ok(new CartSummaryResponse());
        }

        var query = new GetCartQuery
        {
            UserId = userId,
            SessionId = sessionId
        };

        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess || result.Data == null)
        {
            return Ok(new CartSummaryResponse());
        }

        var summary = new CartSummaryResponse
        {
            ItemCount = result.Data.TotalItems,
            TotalAmount = result.Data.TotalAmount,
            Currency = result.Data.Currency
        };

        return Ok(summary);
    }

    /// <summary>
    /// Merge guest cart with user cart (called after login)
    /// </summary>
    /// <param name="request">Guest session details</param>
    /// <returns>Merged cart details</returns>
    /// <response code="200">Carts merged successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("merge")]
    [Authorize]
    [ProducesResponseType(typeof(CartDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> MergeCart([FromBody] MergeCartRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var command = new MergeCartCommand
        {
            UserId = userId.Value,
            SessionId = request.SessionId
        };

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}

public class CartSummaryResponse
{
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
}

public class MergeCartRequest
{
    public string SessionId { get; set; } = string.Empty;
}
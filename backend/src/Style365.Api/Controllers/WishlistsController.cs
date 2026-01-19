using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Features.Wishlists.Commands.AddToWishlist;
using Style365.Application.Features.Wishlists.Commands.RemoveFromWishlist;
using Style365.Application.Features.Wishlists.Queries.CheckWishlistStatus;
using Style365.Application.Features.Wishlists.Queries.GetWishlists;
using System.Security.Claims;

namespace Style365.Api.Controllers;

/// <summary>
/// Wishlist management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WishlistsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current user's wishlists
    /// </summary>
    /// <returns>List of user's wishlists</returns>
    /// <response code="200">Wishlists retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<WishlistDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyWishlists()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var query = new GetWishlistsQuery { UserId = userId.Value };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Add product to wishlist
    /// </summary>
    /// <param name="command">Product and wishlist information</param>
    /// <returns>No content</returns>
    /// <response code="204">Product added to wishlist successfully</response>
    /// <response code="400">Invalid request or product already in wishlist</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("items")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistCommand command)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        command.UserId = userId.Value;
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return NoContent();
    }

    /// <summary>
    /// Remove product from wishlist
    /// </summary>
    /// <param name="productId">Product ID to remove</param>
    /// <param name="wishlistId">Wishlist ID (optional, uses default if not specified)</param>
    /// <returns>No content</returns>
    /// <response code="204">Product removed from wishlist successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized</response>
    [HttpDelete("items/{productId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId, [FromQuery] Guid? wishlistId = null)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var command = new RemoveFromWishlistCommand 
        { 
            UserId = userId.Value, 
            ProductId = productId, 
            WishlistId = wishlistId 
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return NoContent();
    }

    /// <summary>
    /// Check if product is in user's wishlist
    /// </summary>
    /// <param name="productId">Product ID to check</param>
    /// <returns>Wishlist status</returns>
    /// <response code="200">Status retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("items/{productId}/status")]
    [ProducesResponseType(typeof(CheckWishlistStatusResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CheckWishlistStatus(Guid productId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var query = new CheckWishlistStatusQuery 
        { 
            UserId = userId.Value, 
            ProductId = productId 
        };
        
        var result = await _mediator.Send(query);
        
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
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Features.Reviews.Commands.CreateReview;
using Style365.Application.Features.Reviews.Queries.GetReviewsByProduct;
using Style365.Application.Features.Reviews.Queries.GetReviewsByUser;
using Style365.Application.Features.Reviews.Queries.GetReviewStats;
using System.Security.Claims;

namespace Style365.Api.Controllers;

/// <summary>
/// Product reviews management endpoints
/// </summary>
[ApiController]
[Route("api/products/{productId}/reviews")]
public class ProductReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get reviews for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="sortBy">Sort by (rating, created, helpful)</param>
    /// <param name="ascending">Sort direction (default: false)</param>
    /// <returns>Paginated list of product reviews</returns>
    /// <response code="200">Reviews retrieved successfully</response>
    /// <response code="404">Product not found</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetReviewsByProductResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProductReviews(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = false)
    {
        var query = new GetReviewsByProductQuery
        {
            ProductId = productId,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            Ascending = ascending
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new review for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="command">Review details</param>
    /// <returns>Created review details</returns>
    /// <response code="201">Review created successfully</response>
    /// <response code="400">Invalid request or user already reviewed this product</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateReviewResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateReview(Guid productId, [FromBody] CreateReviewCommand command)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        command.UserId = userId.Value;
        command.ProductId = productId;
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return CreatedAtAction(nameof(GetProductReviews), new { productId }, result.Data);
    }

    /// <summary>
    /// Get product review statistics
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Review statistics including average rating and count</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="404">Product not found</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(GetReviewStatsResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetReviewStats(Guid productId)
    {
        var query = new GetReviewStatsQuery { ProductId = productId };
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

/// <summary>
/// User reviews management endpoints
/// </summary>
[ApiController]
[Route("api/users/reviews")]
[Authorize]
public class UserReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current user's reviews
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of user's reviews</returns>
    /// <response code="200">Reviews retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetReviewsByUserResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetUserReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var query = new GetReviewsByUserQuery
        {
            UserId = userId.Value,
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

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
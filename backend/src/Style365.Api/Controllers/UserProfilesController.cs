using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Features.UserProfiles.Commands.UpdateProfile;
using Style365.Application.Features.UserProfiles.Queries.GetProfile;
using System.Security.Claims;

namespace Style365.Api.Controllers;

/// <summary>
/// User profile management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    /// <returns>User profile information</returns>
    /// <response code="200">Profile retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Profile not found</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var query = new GetUserProfileQuery { UserId = userId.Value };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    /// <param name="command">Profile update data</param>
    /// <returns>No content</returns>
    /// <response code="204">Profile updated successfully</response>
    /// <response code="400">Invalid profile data</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("me")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileCommand command)
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
    /// Get user profile by ID (Admin only)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User profile information</returns>
    /// <response code="200">Profile retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Admin access required</response>
    /// <response code="404">Profile not found</response>
    [HttpGet("{userId}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserProfile(Guid userId)
    {
        var query = new GetUserProfileQuery { UserId = userId };
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
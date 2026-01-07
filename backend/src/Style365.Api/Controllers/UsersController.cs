using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Features.Users.Commands.CreateUser;
using Style365.Application.Features.Users.Queries.GetUser;

namespace Style365.Api.Controllers;

/// <summary>
/// User management endpoints for admin operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">Unique user identifier</param>
    /// <returns>User information</returns>
    /// <response code="200">User found and returned</response>
    /// <response code="404">User not found</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var query = new GetUserQuery { UserId = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new user (Admin only)
    /// </summary>
    /// <param name="command">User creation details including email, name, and role</param>
    /// <returns>Created user information</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid user data or email already exists</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetUser), new { id = result.Data!.Id }, result.Data);
    }
}
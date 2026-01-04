using MediatR;
using Microsoft.AspNetCore.Mvc;
using Style365.Application.Features.Users.Commands.CreateUser;
using Style365.Application.Features.Users.Queries.GetUser;

namespace Style365.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
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

    [HttpPost]
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
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Style365.Api.Controllers.Requests;
using Style365.Application.Common.Interfaces;
using Style365.Application.Features.Authentication.Commands.Login;
using Style365.Application.Features.Authentication.Commands.Register;

namespace Style365.Api.Controllers;

/// <summary>
/// Authentication and user session management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="command">User registration details including email, password, and personal information</param>
    /// <returns>Registration result with email confirmation requirements</returns>
    /// <response code="200">Registration successful, check email for confirmation</response>
    /// <response code="400">Invalid input or user already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new
        {
            message = "Registration successful",
            requiresEmailConfirmation = result.Data?.RequiresEmailConfirmation,
            user = result.Data?.User
        });
    }

    /// <summary>
    /// Authenticate user and obtain access token
    /// </summary>
    /// <param name="command">Login credentials (email and password)</param>
    /// <returns>JWT access token and user information</returns>
    /// <response code="200">Login successful, returns JWT token</response>
    /// <response code="401">Invalid credentials or unverified email</response>
    [HttpPost("login")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { errors = result.Errors });
        }

        return Ok(new
        {
            accessToken = result.Data?.AccessToken,
            refreshToken = result.Data?.RefreshToken,
            expiresIn = result.Data?.ExpiresIn,
            tokenType = result.Data?.TokenType,
            user = result.Data?.User
        });
    }

    /// <summary>
    /// Get authenticated user's profile information
    /// </summary>
    /// <returns>User profile data extracted from JWT token</returns>
    /// <response code="200">Profile data retrieved successfully</response>
    /// <response code="401">Invalid or missing JWT token</response>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public Task<IActionResult> GetProfile()
    {
        // Get user ID from JWT claims - Cognito maps it to the standard ClaimTypes.NameIdentifier
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Task.FromResult<IActionResult>(Unauthorized(new { error = "Invalid token" }));
        }

        // Get Cognito username (different from user ID)
        var username = User.FindFirst("username")?.Value;

        // In a real implementation, you'd get the user from the database using the Cognito user ID
        // For now, return the claims from the JWT token
        var userInfo = new
        {
            id = userIdClaim,
            username = username,
            email = User.FindFirst("email")?.Value,
            firstName = User.FindFirst("given_name")?.Value,
            lastName = User.FindFirst("family_name")?.Value,
            groups = User.FindAll("cognito:groups").Select(c => c.Value).ToArray(),
            clientId = User.FindFirst("client_id")?.Value
        };

        return Task.FromResult<IActionResult>(Ok(userInfo));
    }

    /// <summary>
    /// Confirm user email with verification code
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="code">Confirmation code received via email</param>
    /// <returns>Confirmation result</returns>
    /// <response code="200">Email confirmed successfully</response>
    /// <response code="400">Invalid code or email</response>
    [HttpPost("confirm-email")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string code)
    {
        var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
        var result = await authService.ConfirmEmailAsync(email, code);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = "Email confirmed successfully" });
    }

    /// <summary>
    /// Resend email confirmation code
    /// </summary>
    /// <param name="request">User's email address</param>
    /// <returns>Resend result</returns>
    /// <response code="200">Confirmation code sent</response>
    /// <response code="400">Invalid email or user already confirmed</response>
    [HttpPost("resend-confirmation")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResendConfirmationCode([FromBody] ResendConfirmationRequest request)
    {
        var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
        var result = await authService.ResendConfirmationCodeAsync(request.Email);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = "Confirmation code sent to email" });
    }

    /// <summary>
    /// Initiate password reset process
    /// </summary>
    /// <param name="request">User's email address</param>
    /// <returns>Password reset initiation result</returns>
    /// <response code="200">Password reset code sent to email</response>
    /// <response code="400">Invalid email</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
        var result = await authService.ForgotPasswordAsync(request.Email);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = "Password reset code sent to email" });
    }

    /// <summary>
    /// Reset password with confirmation code
    /// </summary>
    /// <param name="request">Email, confirmation code, and new password</param>
    /// <returns>Password reset result</returns>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Invalid code or password requirements not met</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
        var result = await authService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = "Password reset successfully" });
    }

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    /// <param name="request">Current and new password</param>
    /// <returns>Password change result</returns>
    /// <response code="200">Password changed successfully</response>
    /// <response code="400">Invalid current password or new password requirements not met</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var result = await authService.ChangePasswordAsync(accessToken, request.OldPassword, request.NewPassword);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New access token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid or expired refresh token</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
        var result = await authService.RefreshTokenAsync(request.RefreshToken);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new
        {
            accessToken = result.Data?.AccessToken,
            idToken = result.Data?.IdToken,
            expiresIn = result.Data?.ExpiresIn,
            tokenType = result.Data?.TokenType
        });
    }

    /// <summary>
    /// Logout user and invalidate session
    /// </summary>
    /// <returns>Logout confirmation</returns>
    /// <response code="200">Logout successful</response>
    /// <response code="401">Invalid or missing JWT token</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Logout()
    {
        var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var result = await authService.SignOutAsync(accessToken);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }
        
        return Ok(new { message = "Logged out successfully" });
    }
}
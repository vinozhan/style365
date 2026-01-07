namespace Style365.Api.Controllers.Requests;

/// <summary>
/// Request to resend email confirmation code
/// </summary>
public class ResendConfirmationRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request to initiate password reset
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request to reset password with confirmation code
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Confirmation code received via email
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// New password (must meet security requirements)
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Request to change password for authenticated user
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Current password
    /// </summary>
    public string OldPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// New password (must meet security requirements)
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Request to refresh access token
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Refresh token obtained during login
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
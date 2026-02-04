using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Common.Interfaces;

public interface IAuthenticationService
{
    Task<Result<AuthenticationResult>> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<Result<AuthenticationResult>> LoginAsync(string email, string password);
    Task<Result> ConfirmEmailAsync(string email, string confirmationCode);
    Task<Result> ResendConfirmationCodeAsync(string email);
    Task<Result> ForgotPasswordAsync(string email);
    Task<Result> ResetPasswordAsync(string email, string confirmationCode, string newPassword);
    Task<Result> ChangePasswordAsync(string accessToken, string oldPassword, string newPassword);
    Task<Result<AuthenticationResult>> RefreshTokenAsync(string refreshToken);
    Task<Result<User?>> GetUserByAccessTokenAsync(string accessToken);
    Task<Result> SignOutAsync(string accessToken);
    Task<Result> DeleteUserAsync(string cognitoUserId);
}

public class AuthenticationResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string IdToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public User? User { get; set; }
    public bool RequiresEmailConfirmation { get; set; }
}
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using Style365.Domain.Enums;
using Style365.Domain.ValueObjects;
using System.Security.Cryptography;
using System.Text;

namespace Style365.Infrastructure.Authentication;

public class CognitoAuthenticationService : IAuthenticationService
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly AuthSettings _authSettings;
    private readonly IUnitOfWork _unitOfWork;

    public CognitoAuthenticationService(
        IAmazonCognitoIdentityProvider cognitoClient,
        IOptions<AuthSettings> authSettings,
        IUnitOfWork unitOfWork)
    {
        _cognitoClient = cognitoClient;
        _authSettings = authSettings.Value;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResult>> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        try
        {
            var emailObj = Email.Create(email);
            
            // Check if user already exists in our database
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(emailObj);
            if (existingUser != null)
            {
                return Result.Failure<AuthenticationResult>("User already exists");
            }

            // Create user in Cognito
            var signUpRequest = new SignUpRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                //Username = email,
                Username = Guid.NewGuid().ToString(),
                Password = password,
                // SecretHash = ComputeSecretHash(email),
                UserAttributes = new List<AttributeType>
                {
                    new() { Name = "email", Value = email },
                    new() { Name = "given_name", Value = firstName },
                    new() { Name = "family_name", Value = lastName },
                    // new() { Name = "email_verified", Value = "false" }
                }
            };

            var signUpResponse = await _cognitoClient.SignUpAsync(signUpRequest);

            // Create user in our database
            var user = new User(firstName, lastName, email, UserRole.Customer);
            user.SetCognitoUserId(signUpResponse.UserSub);
            
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var result = new AuthenticationResult
            {
                User = user,
                RequiresEmailConfirmation = !(signUpResponse.UserConfirmed ?? false)
            };

            return Result.Success(result);
        }
        catch (UsernameExistsException)
        {
            return Result.Failure<AuthenticationResult>("User already exists");
        }
        catch (InvalidPasswordException ex)
        {
            return Result.Failure<AuthenticationResult>($"Invalid password: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Failure<AuthenticationResult>($"Registration failed: {ex.Message}");
        }
    }

    public async Task<Result<AuthenticationResult>> LoginAsync(string email, string password)
    {
        try
        {
            var authRequest = new InitiateAuthRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", email },
                    { "PASSWORD", password },
                    // { "SECRET_HASH", ComputeSecretHash(email) }
                }
            };

            var authResponse = await _cognitoClient.InitiateAuthAsync(authRequest);

            if (authResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
            {
                return Result.Failure<AuthenticationResult>("New password required");
            }

            // Get Cognito user info first to get the sub (user ID)
            var getUserRequest = new GetUserRequest
            {
                AccessToken = authResponse.AuthenticationResult.AccessToken
            };
            var cognitoUser = await _cognitoClient.GetUserAsync(getUserRequest);
            var cognitoUserId = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "sub")?.Value ?? cognitoUser.Username;

            // Try to find user by CognitoUserId first (most reliable), then by email
            var user = await _unitOfWork.Users.GetByCognitoUserIdAsync(cognitoUserId);

            if (user == null)
            {
                var emailObj = Email.Create(email);
                user = await _unitOfWork.Users.GetByEmailAsync(emailObj);
            }

            if (user != null)
            {
                user.UpdateLastLogin();
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                // User exists in Cognito but not locally - sync them
                var firstName = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "given_name")?.Value ?? "User";
                var lastName = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "family_name")?.Value ?? "User";

                user = new User(firstName, lastName, email, UserRole.Customer);
                user.SetCognitoUserId(cognitoUserId); // This also sets User.Id = cognitoUserId (GUID)
                user.MarkEmailVerified(); // User can login, so email is verified
                user.UpdateLastLogin();

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            var result = new AuthenticationResult
            {
                AccessToken = authResponse.AuthenticationResult.AccessToken,
                RefreshToken = authResponse.AuthenticationResult.RefreshToken,
                IdToken = authResponse.AuthenticationResult.IdToken,
                ExpiresIn = authResponse.AuthenticationResult.ExpiresIn ?? 3600,
                User = user
            };
            

            return Result.Success(result);
        }
        catch (NotAuthorizedException)
        {
            return Result.Failure<AuthenticationResult>("Invalid email or password");
        }
        catch (UserNotConfirmedException)
        {
            return Result.Failure<AuthenticationResult>("Email not confirmed");
        }
        catch (Exception ex)
        {
            return Result.Failure<AuthenticationResult>($"Login failed: {ex.Message}");
        }
    }

    public async Task<Result> ConfirmEmailAsync(string email, string confirmationCode)
    {
        try
        {
            var confirmRequest = new ConfirmSignUpRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                Username = email,
                ConfirmationCode = confirmationCode,
                // SecretHash = ComputeSecretHash(email)
            };

            await _cognitoClient.ConfirmSignUpAsync(confirmRequest);

            // Update user email verification status
            var emailObj = Email.Create(email);
            var user = await _unitOfWork.Users.GetByEmailAsync(emailObj);
            if (user != null)
            {
                user.MarkEmailVerified();
                await _unitOfWork.SaveChangesAsync();
            }

            return Result.Success();
        }
        catch (CodeMismatchException)
        {
            return Result.Failure("Invalid confirmation code");
        }
        catch (ExpiredCodeException)
        {
            return Result.Failure("Confirmation code has expired");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Email confirmation failed: {ex.Message}");
        }
    }

    public async Task<Result> ResendConfirmationCodeAsync(string email)
    {
        try
        {
            var resendRequest = new ResendConfirmationCodeRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                Username = email,
                // SecretHash = ComputeSecretHash(email)
            };

            await _cognitoClient.ResendConfirmationCodeAsync(resendRequest);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to resend confirmation code: {ex.Message}");
        }
    }

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        try
        {
            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                Username = email,
                // SecretHash = ComputeSecretHash(email)
            };

            await _cognitoClient.ForgotPasswordAsync(forgotPasswordRequest);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Forgot password failed: {ex.Message}");
        }
    }

    public async Task<Result> ResetPasswordAsync(string email, string confirmationCode, string newPassword)
    {
        try
        {
            var confirmPasswordRequest = new ConfirmForgotPasswordRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                Username = email,
                ConfirmationCode = confirmationCode,
                Password = newPassword,
                // SecretHash = ComputeSecretHash(email)
            };

            await _cognitoClient.ConfirmForgotPasswordAsync(confirmPasswordRequest);
            return Result.Success();
        }
        catch (CodeMismatchException)
        {
            return Result.Failure("Invalid confirmation code");
        }
        catch (ExpiredCodeException)
        {
            return Result.Failure("Confirmation code has expired");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Password reset failed: {ex.Message}");
        }
    }

    public async Task<Result> ChangePasswordAsync(string accessToken, string oldPassword, string newPassword)
    {
        try
        {
            var changePasswordRequest = new ChangePasswordRequest
            {
                AccessToken = accessToken,
                PreviousPassword = oldPassword,
                ProposedPassword = newPassword
            };

            await _cognitoClient.ChangePasswordAsync(changePasswordRequest);
            return Result.Success();
        }
        catch (NotAuthorizedException)
        {
            return Result.Failure("Invalid current password");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Password change failed: {ex.Message}");
        }
    }

    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var refreshRequest = new InitiateAuthRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "REFRESH_TOKEN", refreshToken }
                }
            };

            var refreshResponse = await _cognitoClient.InitiateAuthAsync(refreshRequest);
            
            var result = new AuthenticationResult
            {
                AccessToken = refreshResponse.AuthenticationResult.AccessToken,
                IdToken = refreshResponse.AuthenticationResult.IdToken,
                ExpiresIn = refreshResponse.AuthenticationResult.ExpiresIn ?? 3600,
                TokenType = refreshResponse.AuthenticationResult.TokenType
            };
            
            return Result.Success(result);
        }
        catch (NotAuthorizedException)
        {
            return Result.Failure<AuthenticationResult>("Invalid or expired refresh token");
        }
        catch (Exception ex)
        {
            return Result.Failure<AuthenticationResult>($"Token refresh failed: {ex.Message}");
        }
    }

    public async Task<Result<User?>> GetUserByAccessTokenAsync(string accessToken)
    {
        try
        {
            var getUserRequest = new GetUserRequest
            {
                AccessToken = accessToken
            };

            var getUserResponse = await _cognitoClient.GetUserAsync(getUserRequest);
            
            var emailAttribute = getUserResponse.UserAttributes.FirstOrDefault(a => a.Name == "email");
            if (emailAttribute == null)
            {
                return Result.Failure<User?>("User email not found");
            }

            var emailObj = Email.Create(emailAttribute.Value);
            var user = await _unitOfWork.Users.GetByEmailAsync(emailObj);

            return Result.Success<User?>(user);
        }
        catch (Exception ex)
        {
            return Result.Failure<User?>($"Failed to get user: {ex.Message}");
        }
    }

    public async Task<Result> SignOutAsync(string accessToken)
    {
        try
        {
            var signOutRequest = new GlobalSignOutRequest
            {
                AccessToken = accessToken
            };

            await _cognitoClient.GlobalSignOutAsync(signOutRequest);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Sign out failed: {ex.Message}");
        }
    }

    // private string ComputeSecretHash(string username)
    // {
    //     var message = username + _authSettings.Cognito.UserPoolClientId;
    //     var keyBytes = Encoding.UTF8.GetBytes(_authSettings.Cognito.UserPoolClientSecret);
    //     var messageBytes = Encoding.UTF8.GetBytes(message);

    //     using var hmac = new HMACSHA256(keyBytes);
    //     var hashBytes = hmac.ComputeHash(messageBytes);
    //     return Convert.ToBase64String(hashBytes);
    // }
}
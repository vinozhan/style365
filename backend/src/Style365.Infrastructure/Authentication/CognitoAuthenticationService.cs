using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using Style365.Domain.Enums;
using Style365.Domain.ValueObjects;

namespace Style365.Infrastructure.Authentication;

public class CognitoAuthenticationService : IAuthenticationService
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly AuthSettings _authSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CognitoAuthenticationService> _logger;

    public CognitoAuthenticationService(
        IAmazonCognitoIdentityProvider cognitoClient,
        IOptions<AuthSettings> authSettings,
        IUnitOfWork unitOfWork,
        ILogger<CognitoAuthenticationService> logger)
    {
        _cognitoClient = cognitoClient;
        _authSettings = authSettings.Value;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResult>> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", email);

            var emailObj = Email.Create(email);

            // Check if user already exists in our database
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(emailObj);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - user already exists: {Email}", email);
                return Result.Failure<AuthenticationResult>("User already exists");
            }

            // Create user in Cognito
            var signUpRequest = new SignUpRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                Username = Guid.NewGuid().ToString(),
                Password = password,
                UserAttributes = new List<AttributeType>
                {
                    new() { Name = "email", Value = email },
                    new() { Name = "given_name", Value = firstName },
                    new() { Name = "family_name", Value = lastName },
                }
            };

            var signUpResponse = await _cognitoClient.SignUpAsync(signUpRequest);

            // Validate Cognito response
            if (string.IsNullOrEmpty(signUpResponse.UserSub))
            {
                _logger.LogError("Cognito registration succeeded but UserSub is missing for email: {Email}", email);
                return Result.Failure<AuthenticationResult>("Registration failed: Invalid response from authentication provider");
            }

            // Validate UserSub is a valid GUID
            if (!Guid.TryParse(signUpResponse.UserSub, out _))
            {
                _logger.LogError("Cognito UserSub is not a valid GUID: {UserSub} for email: {Email}", signUpResponse.UserSub, email);
                return Result.Failure<AuthenticationResult>("Registration failed: Invalid user identifier format");
            }

            // Create user in our database
            var user = new User(firstName, lastName, email, UserRole.Customer);
            user.SetCognitoUserId(signUpResponse.UserSub);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {UserId}, Email: {Email}", user.Id, email);

            var result = new AuthenticationResult
            {
                User = user,
                RequiresEmailConfirmation = !(signUpResponse.UserConfirmed ?? false)
            };

            return Result.Success(result);
        }
        catch (UsernameExistsException)
        {
            _logger.LogWarning("Registration failed - username exists in Cognito: {Email}", email);
            return Result.Failure<AuthenticationResult>("User already exists");
        }
        catch (InvalidPasswordException ex)
        {
            _logger.LogWarning("Registration failed - invalid password for: {Email}. Reason: {Message}", email, ex.Message);
            return Result.Failure<AuthenticationResult>($"Invalid password: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for email: {Email}", email);
            return Result.Failure<AuthenticationResult>($"Registration failed: {ex.Message}");
        }
    }

    public async Task<Result<AuthenticationResult>> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", email);

            var authRequest = new InitiateAuthRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", email },
                    { "PASSWORD", password },
                }
            };

            var authResponse = await _cognitoClient.InitiateAuthAsync(authRequest);

            if (authResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
            {
                _logger.LogInformation("Login requires new password for: {Email}", email);
                return Result.Failure<AuthenticationResult>("New password required");
            }

            // Validate authentication result
            if (authResponse.AuthenticationResult == null)
            {
                _logger.LogError("Cognito authentication succeeded but AuthenticationResult is null for: {Email}", email);
                return Result.Failure<AuthenticationResult>("Login failed: Invalid response from authentication provider");
            }

            if (string.IsNullOrEmpty(authResponse.AuthenticationResult.AccessToken))
            {
                _logger.LogError("Cognito authentication succeeded but AccessToken is missing for: {Email}", email);
                return Result.Failure<AuthenticationResult>("Login failed: No access token received");
            }

            // Get Cognito user info to get the sub (user ID)
            var getUserRequest = new GetUserRequest
            {
                AccessToken = authResponse.AuthenticationResult.AccessToken
            };
            var cognitoUser = await _cognitoClient.GetUserAsync(getUserRequest);

            // Extract and validate Cognito sub
            var cognitoUserId = ExtractCognitoAttribute(cognitoUser.UserAttributes, "sub");
            if (string.IsNullOrEmpty(cognitoUserId))
            {
                _logger.LogError("Cognito user missing 'sub' attribute for: {Email}", email);
                return Result.Failure<AuthenticationResult>("Login failed: User identifier not found");
            }

            if (!Guid.TryParse(cognitoUserId, out _))
            {
                _logger.LogError("Cognito 'sub' is not a valid GUID: {Sub} for: {Email}", cognitoUserId, email);
                return Result.Failure<AuthenticationResult>("Login failed: Invalid user identifier format");
            }

            // Try to find user by CognitoUserId first (most reliable), then by email
            var user = await _unitOfWork.Users.GetByCognitoUserIdAsync(cognitoUserId);

            if (user == null)
            {
                var emailObj = Email.Create(email);
                user = await _unitOfWork.Users.GetByEmailAsync(emailObj);

                // If user exists by email but doesn't have CognitoUserId, link them
                if (user != null && string.IsNullOrEmpty(user.CognitoUserId))
                {
                    _logger.LogInformation("Linking existing user {UserId} to Cognito: {CognitoUserId}", user.Id, cognitoUserId);
                    user.SetCognitoUserId(cognitoUserId);
                }
            }

            if (user == null)
            {
                // User exists in Cognito but not locally
                // Instead of silently creating, return an error with clear message
                _logger.LogWarning("User exists in Cognito but not in local database: {Email}, CognitoId: {CognitoUserId}", email, cognitoUserId);
                return Result.Failure<AuthenticationResult>(
                    "Account not found. Please register first or contact support if you believe this is an error.");
            }

            // Sync role from Cognito groups (use Username, not sub)
            var cognitoRole = await GetRoleFromCognitoGroupsAsync(cognitoUser.Username);
            if (user.Role != cognitoRole)
            {
                _logger.LogInformation("Syncing role for user {UserId}: {OldRole} -> {NewRole}", user.Id, user.Role, cognitoRole);
                user.UpdateRole(cognitoRole);
            }

            // Update last login
            user.UpdateLastLogin();
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User logged in successfully: {UserId}, Email: {Email}", user.Id, email);

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
            _logger.LogWarning("Login failed - invalid credentials for: {Email}", email);
            return Result.Failure<AuthenticationResult>("Invalid email or password");
        }
        catch (UserNotConfirmedException)
        {
            _logger.LogWarning("Login failed - email not confirmed for: {Email}", email);
            return Result.Failure<AuthenticationResult>("Email not confirmed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for email: {Email}", email);
            return Result.Failure<AuthenticationResult>($"Login failed: {ex.Message}");
        }
    }

    public async Task<Result> ConfirmEmailAsync(string email, string confirmationCode)
    {
        try
        {
            _logger.LogInformation("Email confirmation attempt for: {Email}", email);

            var confirmRequest = new ConfirmSignUpRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                Username = email,
                ConfirmationCode = confirmationCode,
            };

            await _cognitoClient.ConfirmSignUpAsync(confirmRequest);

            // Update user email verification status
            var emailObj = Email.Create(email);
            var user = await _unitOfWork.Users.GetByEmailAsync(emailObj);
            if (user != null)
            {
                user.MarkEmailVerified();
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Email confirmed for user: {UserId}, Email: {Email}", user.Id, email);
            }
            else
            {
                _logger.LogWarning("Email confirmed in Cognito but user not found locally: {Email}", email);
            }

            return Result.Success();
        }
        catch (CodeMismatchException)
        {
            _logger.LogWarning("Email confirmation failed - invalid code for: {Email}", email);
            return Result.Failure("Invalid confirmation code");
        }
        catch (ExpiredCodeException)
        {
            _logger.LogWarning("Email confirmation failed - expired code for: {Email}", email);
            return Result.Failure("Confirmation code has expired");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email confirmation failed for: {Email}", email);
            return Result.Failure($"Email confirmation failed: {ex.Message}");
        }
    }

    public async Task<Result> ResendConfirmationCodeAsync(string email)
    {
        try
        {
            _logger.LogInformation("Resend confirmation code request for: {Email}", email);

            var resendRequest = new ResendConfirmationCodeRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                Username = email,
            };

            await _cognitoClient.ResendConfirmationCodeAsync(resendRequest);

            _logger.LogInformation("Confirmation code resent for: {Email}", email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend confirmation code for: {Email}", email);
            return Result.Failure($"Failed to resend confirmation code: {ex.Message}");
        }
    }

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        try
        {
            _logger.LogInformation("Forgot password request for: {Email}", email);

            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                Username = email,
            };

            await _cognitoClient.ForgotPasswordAsync(forgotPasswordRequest);

            _logger.LogInformation("Password reset code sent for: {Email}", email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Forgot password failed for: {Email}", email);
            return Result.Failure($"Forgot password failed: {ex.Message}");
        }
    }

    public async Task<Result> ResetPasswordAsync(string email, string confirmationCode, string newPassword)
    {
        try
        {
            _logger.LogInformation("Password reset attempt for: {Email}", email);

            var confirmPasswordRequest = new ConfirmForgotPasswordRequest
            {
                ClientId = _authSettings.Cognito.UserPoolClientId,
                Username = email,
                ConfirmationCode = confirmationCode,
                Password = newPassword,
            };

            await _cognitoClient.ConfirmForgotPasswordAsync(confirmPasswordRequest);

            _logger.LogInformation("Password reset successful for: {Email}", email);
            return Result.Success();
        }
        catch (CodeMismatchException)
        {
            _logger.LogWarning("Password reset failed - invalid code for: {Email}", email);
            return Result.Failure("Invalid confirmation code");
        }
        catch (ExpiredCodeException)
        {
            _logger.LogWarning("Password reset failed - expired code for: {Email}", email);
            return Result.Failure("Confirmation code has expired");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset failed for: {Email}", email);
            return Result.Failure($"Password reset failed: {ex.Message}");
        }
    }

    public async Task<Result> ChangePasswordAsync(string accessToken, string oldPassword, string newPassword)
    {
        try
        {
            _logger.LogInformation("Password change request");

            var changePasswordRequest = new ChangePasswordRequest
            {
                AccessToken = accessToken,
                PreviousPassword = oldPassword,
                ProposedPassword = newPassword
            };

            await _cognitoClient.ChangePasswordAsync(changePasswordRequest);

            _logger.LogInformation("Password changed successfully");
            return Result.Success();
        }
        catch (NotAuthorizedException)
        {
            _logger.LogWarning("Password change failed - invalid current password");
            return Result.Failure("Invalid current password");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password change failed");
            return Result.Failure($"Password change failed: {ex.Message}");
        }
    }

    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            _logger.LogInformation("Token refresh request");

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("Token refresh failed - empty refresh token");
                return Result.Failure<AuthenticationResult>("Refresh token is required");
            }

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

            if (refreshResponse.AuthenticationResult == null)
            {
                _logger.LogError("Token refresh succeeded but AuthenticationResult is null");
                return Result.Failure<AuthenticationResult>("Token refresh failed: Invalid response");
            }

            var result = new AuthenticationResult
            {
                AccessToken = refreshResponse.AuthenticationResult.AccessToken,
                IdToken = refreshResponse.AuthenticationResult.IdToken,
                ExpiresIn = refreshResponse.AuthenticationResult.ExpiresIn ?? 3600,
                TokenType = refreshResponse.AuthenticationResult.TokenType
            };

            _logger.LogInformation("Token refreshed successfully");
            return Result.Success(result);
        }
        catch (NotAuthorizedException)
        {
            _logger.LogWarning("Token refresh failed - invalid or expired refresh token");
            return Result.Failure<AuthenticationResult>("Invalid or expired refresh token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return Result.Failure<AuthenticationResult>($"Token refresh failed: {ex.Message}");
        }
    }

    public async Task<Result<User?>> GetUserByAccessTokenAsync(string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Result.Failure<User?>("Access token is required");
            }

            var getUserRequest = new GetUserRequest
            {
                AccessToken = accessToken
            };

            var getUserResponse = await _cognitoClient.GetUserAsync(getUserRequest);

            var emailAttribute = ExtractCognitoAttribute(getUserResponse.UserAttributes, "email");
            if (string.IsNullOrEmpty(emailAttribute))
            {
                _logger.LogWarning("User email not found in Cognito attributes");
                return Result.Failure<User?>("User email not found");
            }

            var emailObj = Email.Create(emailAttribute);
            var user = await _unitOfWork.Users.GetByEmailAsync(emailObj);

            return Result.Success<User?>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by access token");
            return Result.Failure<User?>($"Failed to get user: {ex.Message}");
        }
    }

    public async Task<Result> SignOutAsync(string accessToken)
    {
        try
        {
            _logger.LogInformation("Sign out request");

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Result.Failure("Access token is required");
            }

            var signOutRequest = new GlobalSignOutRequest
            {
                AccessToken = accessToken
            };

            await _cognitoClient.GlobalSignOutAsync(signOutRequest);

            _logger.LogInformation("User signed out successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sign out failed");
            return Result.Failure($"Sign out failed: {ex.Message}");
        }
    }

    public async Task<Result> DeleteUserAsync(string cognitoUserId)
    {
        try
        {
            _logger.LogInformation("Delete user request for CognitoUserId: {CognitoUserId}", cognitoUserId);

            if (string.IsNullOrWhiteSpace(cognitoUserId))
            {
                return Result.Failure("Cognito user ID is required");
            }

            // Delete user from Cognito using AdminDeleteUser
            // Note: We use the cognitoUserId (sub) as the username since that's how we registered users
            var deleteRequest = new AdminDeleteUserRequest
            {
                UserPoolId = _authSettings.Cognito.UserPoolId,
                Username = cognitoUserId
            };

            await _cognitoClient.AdminDeleteUserAsync(deleteRequest);

            _logger.LogInformation("User deleted from Cognito successfully: {CognitoUserId}", cognitoUserId);
            return Result.Success();
        }
        catch (UserNotFoundException)
        {
            _logger.LogWarning("User not found in Cognito during delete: {CognitoUserId}", cognitoUserId);
            // Return success since the user doesn't exist in Cognito anyway
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user from Cognito: {CognitoUserId}", cognitoUserId);
            return Result.Failure($"Failed to delete user from Cognito: {ex.Message}");
        }
    }

    /// <summary>
    /// Safely extracts an attribute value from Cognito user attributes
    /// </summary>
    private static string? ExtractCognitoAttribute(List<AttributeType>? attributes, string attributeName)
    {
        if (attributes == null || attributes.Count == 0)
            return null;

        return attributes.FirstOrDefault(a =>
            string.Equals(a.Name, attributeName, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    /// <summary>
    /// Determines the user role based on Cognito group membership.
    /// Checks groups via AdminListGroupsForUser API.
    /// </summary>
    /// <param name="cognitoUsername">The Cognito username (not the sub/user ID)</param>
    private async Task<UserRole> GetRoleFromCognitoGroupsAsync(string cognitoUsername)
    {
        try
        {
            if (string.IsNullOrEmpty(cognitoUsername))
                return UserRole.Customer;

            // Get user's groups from Cognito
            var listGroupsRequest = new AdminListGroupsForUserRequest
            {
                UserPoolId = _authSettings.Cognito.UserPoolId,
                Username = cognitoUsername
            };

            var groupsResponse = await _cognitoClient.AdminListGroupsForUserAsync(listGroupsRequest);
            var groups = groupsResponse.Groups.Select(g => g.GroupName).ToList();

            _logger.LogDebug("Cognito user {Username} belongs to groups: {Groups}", cognitoUsername, string.Join(", ", groups));

            // Map Cognito groups to UserRole (priority: SuperAdmin > Admin > ContentManager > Customer)
            if (groups.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
                return UserRole.SuperAdmin;
            if (groups.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                return UserRole.Admin;
            if (groups.Contains("ContentManager", StringComparer.OrdinalIgnoreCase))
                return UserRole.ContentManager;

            return UserRole.Customer;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get Cognito groups for username {Username}, defaulting to Customer role", cognitoUsername);
            return UserRole.Customer;
        }
    }
}

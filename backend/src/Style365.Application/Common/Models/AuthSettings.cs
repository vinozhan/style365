namespace Style365.Application.Common.Models;

public class AuthSettings
{
    public const string SectionName = "Authentication";
    
    public CognitoSettings Cognito { get; set; } = new();
    public JwtSettings Jwt { get; set; } = new();
}

public class CognitoSettings
{
    public string Region { get; set; } = string.Empty;
    public string UserPoolId { get; set; } = string.Empty;
    public string UserPoolClientId { get; set; } = string.Empty;
    public string UserPoolClientSecret { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
}

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; } = 60;
}
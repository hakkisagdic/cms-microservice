using IdentityService.Core.DTOs.User;

namespace IdentityService.Core.DTOs;

public class AuthenticationResult
{
    public bool IsSuccess { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserResponse? User { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? Message { get; set; }
    
    public static AuthenticationResult Success(string accessToken, string refreshToken, DateTime expiresAt, UserResponse user)
    {
        return new AuthenticationResult
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = user
        };
    }
    
    public static AuthenticationResult Failure(string error)
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            Errors = new List<string> { error }
        };
    }
    
    public static AuthenticationResult Failure(List<string> errors)
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            Errors = errors
        };
    }
}

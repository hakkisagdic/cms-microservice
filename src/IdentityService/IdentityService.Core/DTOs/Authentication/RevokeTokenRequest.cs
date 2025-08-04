namespace IdentityService.Core.DTOs.Authentication;

public class RevokeTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

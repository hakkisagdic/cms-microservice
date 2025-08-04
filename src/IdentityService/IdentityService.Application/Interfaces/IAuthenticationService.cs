using IdentityService.Core.DTOs;
using IdentityService.Core.DTOs.Authentication;

namespace IdentityService.Application.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync(LoginRequest request, string ipAddress, string? userAgent = null);
    Task<AuthenticationResult> RegisterAsync(RegisterRequest request, string ipAddress, string? userAgent = null);
    Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress, string? userAgent = null);
    Task<bool> RevokeTokenAsync(string token, string ipAddress, string? userAgent = null);
    Task<bool> LogoutAsync(string userId, string ipAddress, string? userAgent = null);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request, string userId, string ipAddress, string? userAgent = null);
}

using IdentityService.Core.DTOs.Authentication;

namespace IdentityService.Infrastructure.Services.Interfaces;

public interface IJwtService
{
    string GenerateJwtToken(string userId, string email, string firstName, string lastName, IList<string> roles);
    string GenerateRefreshToken();
    string? GetUserIdFromToken(string token);
    string? GetJwtIdFromToken(string token);
    bool IsTokenExpired(string token);
    DateTime GetTokenExpiration(string token);
}

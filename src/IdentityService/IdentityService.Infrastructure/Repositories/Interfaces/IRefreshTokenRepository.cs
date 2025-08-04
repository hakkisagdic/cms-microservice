using IdentityService.Core.Entities;

namespace IdentityService.Infrastructure.Repositories.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken?> GetActiveTokenByUserIdAsync(string userId);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
    Task RevokeAllUserTokensAsync(string userId, string? reason = null);
    Task<RefreshToken?> GetByJwtIdAsync(string jwtId);
    Task CleanupExpiredTokensAsync();
}

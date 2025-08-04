using Microsoft.EntityFrameworkCore;
using IdentityService.Core.Entities;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Repositories.Interfaces;

namespace IdentityService.Infrastructure.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<RefreshToken?> GetActiveTokenByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.UserId == userId && 
                                     !rt.IsRevoked && 
                                     !rt.IsUsed && 
                                     rt.ExpiryDate > DateTime.UtcNow);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .Where(rt => rt.UserId == userId && 
                        !rt.IsRevoked && 
                        !rt.IsUsed && 
                        rt.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task RevokeAllUserTokensAsync(string userId, string? reason = null)
    {
        var tokens = await _dbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedReason = reason ?? "All tokens revoked";
        }

        _dbSet.UpdateRange(tokens);
    }

    public async Task<RefreshToken?> GetByJwtIdAsync(string jwtId)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.JwtId == jwtId);
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _dbSet
            .Where(rt => rt.ExpiryDate < DateTime.UtcNow || rt.IsRevoked || rt.IsUsed)
            .Where(rt => rt.CreatedAt < DateTime.UtcNow.AddDays(-30)) // Keep for 30 days for audit
            .ToListAsync();

        _dbSet.RemoveRange(expiredTokens);
    }
}

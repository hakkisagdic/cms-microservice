using Microsoft.EntityFrameworkCore;
using IdentityService.Core.Entities;
using IdentityService.Core.Enums;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Repositories.Interfaces;

namespace IdentityService.Infrastructure.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int skip = 0, int take = 50)
    {
        return await _dbSet
            .Include(al => al.User)
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action, int skip = 0, int take = 50)
    {
        return await _dbSet
            .Include(al => al.User)
            .Where(al => al.Action == action)
            .OrderByDescending(al => al.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50)
    {
        return await _dbSet
            .Include(al => al.User)
            .Where(al => al.Timestamp >= startDate && al.Timestamp <= endDate)
            .OrderByDescending(al => al.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetFailedAttemptsAsync(string? userId = null, int skip = 0, int take = 50)
    {
        var query = _dbSet
            .Include(al => al.User)
            .Where(al => !al.IsSuccessful);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(al => al.UserId == userId);
        }

        return await query
            .OrderByDescending(al => al.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task CleanupOldLogsAsync(int daysToKeep = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        var oldLogs = await _dbSet
            .Where(al => al.Timestamp < cutoffDate)
            .ToListAsync();

        _dbSet.RemoveRange(oldLogs);
    }

    public async Task<int> GetFailedAttemptCountAsync(string userId, DateTime since)
    {
        return await _dbSet
            .CountAsync(al => al.UserId == userId &&
                             !al.IsSuccessful &&
                             al.Timestamp >= since);
    }
}

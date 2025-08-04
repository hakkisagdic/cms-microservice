using IdentityService.Core.Entities;
using IdentityService.Core.Enums;

namespace IdentityService.Infrastructure.Repositories.Interfaces;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int skip = 0, int take = 50);
    Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action, int skip = 0, int take = 50);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50);
    Task<IEnumerable<AuditLog>> GetFailedAttemptsAsync(string? userId = null, int skip = 0, int take = 50);
    Task CleanupOldLogsAsync(int daysToKeep = 90);
    Task<int> GetFailedAttemptCountAsync(string userId, DateTime since);
}

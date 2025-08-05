using IdentityService.Core.DTOs.Audit;
using IdentityService.Core.Entities;
using IdentityService.Core.Enums;
using IdentityService.Infrastructure.Repositories.Interfaces;
using IdentityService.Infrastructure.Services.Interfaces;

namespace IdentityService.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(AuditLogRequest request)
    {
        var auditLog = new AuditLog
        {
            UserId = request.UserId,
            UserEmail = request.UserEmail,
            Action = request.Action,
            Resource = request.Resource,
            IsSuccessful = request.IsSuccess,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            ErrorMessage = request.ErrorMessage,
            Timestamp = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(auditLog);
        await _auditLogRepository.SaveChangesAsync();
    }

    public async Task LogLoginAttemptAsync(string userId, string userEmail, bool isSuccess,
                                          string ipAddress, string? userAgent = null, string? errorMessage = null)
    {
        var request = new AuditLogRequest
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = AuditAction.Login,
            Resource = "Authentication",
            IsSuccess = isSuccess,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ErrorMessage = errorMessage
        };

        await LogAsync(request);
    }

    public async Task LogLogoutAsync(string userId, string userEmail, string ipAddress, string? userAgent = null)
    {
        var request = new AuditLogRequest
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = AuditAction.Logout,
            Resource = "Authentication",
            IsSuccess = true,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await LogAsync(request);
    }

    public async Task LogRegistrationAsync(string userId, string userEmail, bool isSuccess,
                                          string ipAddress, string? userAgent = null, string? errorMessage = null)
    {
        var request = new AuditLogRequest
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = AuditAction.Register,
            Resource = "User",
            IsSuccess = isSuccess,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ErrorMessage = errorMessage
        };

        await LogAsync(request);
    }

    public async Task LogPasswordChangeAsync(string userId, string userEmail, bool isSuccess,
                                           string ipAddress, string? userAgent = null, string? errorMessage = null)
    {
        var request = new AuditLogRequest
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = AuditAction.PasswordChange,
            Resource = "User",
            IsSuccess = isSuccess,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ErrorMessage = errorMessage
        };

        await LogAsync(request);
    }

    public async Task LogProfileUpdateAsync(string userId, string userEmail, bool isSuccess,
                                          string ipAddress, string? userAgent = null, string? errorMessage = null)
    {
        var request = new AuditLogRequest
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = AuditAction.ProfileUpdate,
            Resource = "User",
            IsSuccess = isSuccess,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ErrorMessage = errorMessage
        };

        await LogAsync(request);
    }
}

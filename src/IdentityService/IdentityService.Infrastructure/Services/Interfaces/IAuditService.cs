using IdentityService.Core.DTOs.Audit;

namespace IdentityService.Infrastructure.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditLogRequest request);

    Task LogLoginAttemptAsync(string userId, string userEmail, bool isSuccess,
                             string ipAddress, string? userAgent = null, string? errorMessage = null);

    Task LogLogoutAsync(string userId, string userEmail, string ipAddress, string? userAgent = null);

    Task LogRegistrationAsync(string userId, string userEmail, bool isSuccess,
                             string ipAddress, string? userAgent = null, string? errorMessage = null);

    Task LogPasswordChangeAsync(string userId, string userEmail, bool isSuccess,
                               string ipAddress, string? userAgent = null, string? errorMessage = null);

    Task LogProfileUpdateAsync(string userId, string userEmail, bool isSuccess,
                              string ipAddress, string? userAgent = null, string? errorMessage = null);
}

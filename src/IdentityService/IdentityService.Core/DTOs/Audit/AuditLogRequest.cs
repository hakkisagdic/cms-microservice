using IdentityService.Core.Enums;

namespace IdentityService.Core.DTOs.Audit;

public class AuditLogRequest
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string Resource { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? ErrorMessage { get; set; }
}

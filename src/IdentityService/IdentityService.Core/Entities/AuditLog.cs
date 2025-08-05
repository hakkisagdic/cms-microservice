using IdentityService.Core.Enums;

namespace IdentityService.Core.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public AuditAction Action { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public bool IsSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? AdditionalData { get; set; }

    public virtual ApplicationUser? User { get; set; }
}

namespace UserService.Core.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
    
    // Navigation properties for potential future extensions
    public string? Department { get; set; }
    public string? Position { get; set; }
}

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Pending = 4
}

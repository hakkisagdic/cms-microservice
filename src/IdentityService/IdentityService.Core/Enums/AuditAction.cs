namespace IdentityService.Core.Enums;

public enum AuditAction
{
    Login = 1,
    Logout = 2,
    Register = 3,
    PasswordChange = 4,
    PasswordReset = 5,
    EmailVerification = 6,
    ProfileUpdate = 7,
    AccountLocked = 8,
    AccountUnlocked = 9,
    TokenRefresh = 10,
    TokenRevoke = 11,
    RoleAssigned = 12,
    RoleRemoved = 13,
    PermissionGranted = 14,
    PermissionRevoked = 15,
    AccountDeactivated = 16,
    AccountActivated = 17,
    TwoFactorEnabled = 18,
    TwoFactorDisabled = 19,
    SocialLoginLinked = 20,
    SocialLoginUnlinked = 21
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UserService.Tests.Helpers;

public static class TestJwtTokenHelper
{
    private const string Secret = "MySecret123456789012345678901234567890";
    private const string Issuer = "IdentityService";
    private const string Audience = "CMSMicroservices";
    private const string KeyId = "cms-key-1";

    public static string GenerateTestToken(
        string userId = "test-user-id",
        string email = "test@example.com",
        string role = "User",
        TimeSpan? expiry = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(Secret);
        var signingKey = new SymmetricSecurityKey(key) { KeyId = KeyId };

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new("user_id", userId),
            new("email", email)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromHours(1)),
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static string GenerateAdminToken(
        string userId = "admin-user-id",
        string email = "admin@example.com")
    {
        return GenerateTestToken(userId, email, "Admin");
    }

    public static string GenerateExpiredToken(
        string userId = "expired-user-id",
        string email = "expired@example.com")
    {
        return GenerateTestToken(userId, email, "User", TimeSpan.FromMinutes(-10));
    }
}

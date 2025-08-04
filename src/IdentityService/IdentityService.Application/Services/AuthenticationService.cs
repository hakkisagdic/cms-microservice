using Microsoft.AspNetCore.Identity;
using IdentityService.Application.Interfaces;
using IdentityService.Core.DTOs;
using IdentityService.Core.DTOs.Authentication;
using IdentityService.Core.DTOs.User;
using IdentityService.Core.Entities;
using IdentityService.Core.Enums;
using IdentityService.Infrastructure.Repositories.Interfaces;
using IdentityService.Infrastructure.Services.Interfaces;

namespace IdentityService.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuditService _auditService;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        IAuditService auditService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
        _auditService = auditService;
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request, string ipAddress, string? userAgent = null)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                await _auditService.LogLoginAttemptAsync(
                    request.Email, request.Email, false, ipAddress, userAgent, "User not found");
                
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    Errors = new List<string> { "Invalid email or password" }
                };
            }

            if (user.Status != UserStatus.Active)
            {
                await _auditService.LogLoginAttemptAsync(
                    user.Id, user.Email!, false, ipAddress, userAgent, "User account is not active");
                
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    Errors = new List<string> { "User account is not active" }
                };
            }

            var result = await _userManager.CheckPasswordAsync(user, request.Password);
            
            if (!result)
            {
                string errorMessage;
                if (user.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    errorMessage = "Account is locked out";
                }
                else if (!user.EmailConfirmed)
                {
                    errorMessage = "Email not confirmed";
                }
                else
                {
                    errorMessage = "Invalid email or password";
                }

                await _auditService.LogLoginAttemptAsync(
                    user.Id, user.Email!, false, ipAddress, userAgent, errorMessage);

                return new AuthenticationResult
                {
                    IsSuccess = false,
                    Errors = new List<string> { errorMessage }
                };
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate tokens
            var jwtToken = _jwtService.GenerateJwtToken(
                user.Id, user.Email!, user.FirstName, user.LastName, roles);
            
            var refreshToken = _jwtService.GenerateRefreshToken();
            var jwtId = _jwtService.GetJwtIdFromToken(jwtToken);

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                JwtId = jwtId!,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30), // 30 days
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            await _auditService.LogLoginAttemptAsync(
                user.Id, user.Email!, true, ipAddress, userAgent);

            return new AuthenticationResult
            {
                IsSuccess = true,
                AccessToken = jwtToken,
                RefreshToken = refreshToken,
                User = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfileImageUrl = user.ProfileImageUrl,
                    Roles = roles.ToList()
                }
            };
        }
        catch (Exception ex)
        {
            await _auditService.LogLoginAttemptAsync(
                request.Email, request.Email, false, ipAddress, userAgent, ex.Message);

            return new AuthenticationResult
            {
                IsSuccess = false,
                Errors = new List<string> { "An error occurred during login" }
            };
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest request, string ipAddress, string? userAgent = null)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                await _auditService.LogRegistrationAsync(
                    request.Email, request.Email, false, ipAddress, userAgent, "Email already exists");

                return new AuthenticationResult
                {
                    IsSuccess = false,
                    Errors = new List<string> { "Email is already registered" }
                };
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                await _auditService.LogRegistrationAsync(
                    request.Email, request.Email, false, ipAddress, userAgent, errors);

                return new AuthenticationResult
                {
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            await _auditService.LogRegistrationAsync(
                user.Id, user.Email, true, ipAddress, userAgent);

            return new AuthenticationResult
            {
                IsSuccess = true,
                Message = "Registration successful"
            };
        }
        catch (Exception ex)
        {
            await _auditService.LogRegistrationAsync(
                request.Email, request.Email, false, ipAddress, userAgent, ex.Message);

            return new AuthenticationResult
            {
                IsSuccess = false,
                Errors = new List<string> { "An error occurred during registration" }
            };
        }
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress, string? userAgent = null)
    {
        try
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            
            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.IsUsed || 
                refreshToken.ExpiryDate < DateTime.UtcNow)
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    Errors = new List<string> { "Invalid or expired refresh token" }
                };
            }

            // Mark the refresh token as used
            refreshToken.IsUsed = true;
            _refreshTokenRepository.Update(refreshToken);

            var user = refreshToken.User;
            if (user == null || user.Status != UserStatus.Active)
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    Errors = new List<string> { "User account is not active" }
                };
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate new tokens
            var jwtToken = _jwtService.GenerateJwtToken(
                user.Id, user.Email!, user.FirstName, user.LastName, roles);
            
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var jwtId = _jwtService.GetJwtIdFromToken(jwtToken);

            // Save new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                JwtId = jwtId!,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            return new AuthenticationResult
            {
                IsSuccess = true,
                AccessToken = jwtToken,
                RefreshToken = newRefreshToken,
                User = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfileImageUrl = user.ProfileImageUrl,
                    Roles = roles.ToList()
                }
            };
        }
        catch (Exception)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                Errors = new List<string> { "An error occurred during token refresh" }
            };
        }
    }

    public async Task<bool> RevokeTokenAsync(string token, string ipAddress, string? userAgent = null)
    {
        try
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
            
            if (refreshToken == null || refreshToken.IsRevoked)
                return false;

            refreshToken.IsRevoked = true;
            refreshToken.RevokedReason = "Token revoked by user";

            _refreshTokenRepository.Update(refreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LogoutAsync(string userId, string ipAddress, string? userAgent = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Revoke all refresh tokens for the user
            await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, "User logged out");
            await _refreshTokenRepository.SaveChangesAsync();

            await _auditService.LogLogoutAsync(userId, user.Email!, ipAddress, userAgent);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request, string userId, string ipAddress, string? userAgent = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            
            if (result.Succeeded)
            {
                await _auditService.LogPasswordChangeAsync(
                    userId, user.Email!, true, ipAddress, userAgent);

                // Revoke all refresh tokens to force re-login
                await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, "Password changed");
                await _refreshTokenRepository.SaveChangesAsync();

                return true;
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                await _auditService.LogPasswordChangeAsync(
                    userId, user.Email!, false, ipAddress, userAgent, errors);

                return false;
            }
        }
        catch (Exception ex)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _auditService.LogPasswordChangeAsync(
                    userId, user.Email!, false, ipAddress, userAgent, ex.Message);
            }

            return false;
        }
    }
}

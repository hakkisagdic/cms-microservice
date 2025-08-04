using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using IdentityService.Application.Interfaces;
using IdentityService.Core.DTOs;
using IdentityService.Core.DTOs.Authentication;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();

        var result = await _authenticationService.LoginAsync(request, ipAddress, userAgent);

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        return Ok(new
        {
            message = "Login successful",
            token = result.AccessToken,
            refreshToken = result.RefreshToken,
            user = result.User
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();

        var result = await _authenticationService.RegisterAsync(request, ipAddress, userAgent);

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = result.Message });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();

        var result = await _authenticationService.RefreshTokenAsync(request, ipAddress, userAgent);

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        return Ok(new
        {
            token = result.AccessToken,
            refreshToken = result.RefreshToken,
            user = result.User
        });
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();

        var success = await _authenticationService.RevokeTokenAsync(request.Token, ipAddress, userAgent);

        if (!success)
            return BadRequest(new { message = "Token not found or already revoked" });

        return Ok(new { message = "Token revoked successfully" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();

        var success = await _authenticationService.LogoutAsync(request.UserId, ipAddress, userAgent);

        if (!success)
            return BadRequest(new { message = "Logout failed" });

        return Ok(new { message = "Logout successful" });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // In a real application, you would get the user ID from the JWT token
        // For now, we'll assume it's passed in the request
        var userId = GetUserIdFromToken();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();

        var success = await _authenticationService.ChangePasswordAsync(request, userId, ipAddress, userAgent);

        if (!success)
            return BadRequest(new { message = "Password change failed" });

        return Ok(new { message = "Password changed successfully" });
    }

    private string GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string? GetUserAgent()
    {
        return HttpContext.Request.Headers.UserAgent.FirstOrDefault();
    }

    private string? GetUserIdFromToken()
    {
        return HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}

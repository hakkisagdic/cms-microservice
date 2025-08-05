using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityService.Application.Interfaces;
using IdentityService.Core.DTOs.User;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private const string UserNotFoundMessage = "User not found";

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserIdFromToken();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = UserNotFoundMessage });

        return Ok(user);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { message = UserNotFoundMessage });

        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null)
            return NotFound(new { message = UserNotFoundMessage });

        return Ok(user);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserIdFromToken();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var updatedUser = await _userService.UpdateUserProfileAsync(userId, request);
            return Ok(updatedUser);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var success = await _userService.DeleteUserAsync(id);
        if (!success)
            return NotFound(new { message = UserNotFoundMessage });

        return Ok(new { message = "User deleted successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        if (take > 100)
            take = 100;

        var users = await _userService.GetUsersAsync(skip, take);
        return Ok(users);
    }

    [HttpGet("check-email/{email}")]
    public async Task<IActionResult> CheckEmailAvailability(string email)
    {
        var isTaken = await _userService.IsEmailTakenAsync(email);
        return Ok(new { isAvailable = !isTaken });
    }

    [HttpGet("check-username/{username}")]
    public async Task<IActionResult> CheckUsernameAvailability(string username)
    {
        var isTaken = await _userService.IsUsernameTakenAsync(username);
        return Ok(new { isAvailable = !isTaken });
    }

    private string? GetUserIdFromToken()
    {
        return HttpContext.User?.FindFirst("sub")?.Value ??
               HttpContext.User?.FindFirst("id")?.Value;
    }
}

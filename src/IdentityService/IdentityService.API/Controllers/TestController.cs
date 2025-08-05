using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok(new { message = "This is a public endpoint" });
    }

    [HttpGet("protected")]
    [Authorize]
    public IActionResult Protected()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.GivenName)?.Value;

        return Ok(new {
            message = "This is a protected endpoint",
            userId = userId,
            email = email,
            name = name,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }
}

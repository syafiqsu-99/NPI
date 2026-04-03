using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPI.Server.Models;
using NPI.Server.Services;
using System.Security.Claims;

namespace NPI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    protected int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Invalid user identity claim.");
        return userId;
    }

    protected string GetCurrentUserRole()
        => User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, token, user) = await _authService.LoginAsync(request.Username, request.Password);

        if (!success)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        return Ok(new
        {
            token,
            user = new
            {
                user.user_id,
                user.username,
                user.email,
                user.full_name,
                department = user.Department?.dept_name,
                departmentId = user.dept_id,
                role = user.Role?.role_name,
                roleId = user.role_id
            }
        });
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new Users
        {
            username = request.Username,
            email = request.Email,
            full_name = request.FullName,
            dept_id = request.DeptId,
            role_id = request.RoleId
        };

        var (success, message) = await _authService.RegisterAsync(user, request.Password);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        var sessionId = User.FindFirst("SessionId")?.Value;

        await _authService.LogoutAsync(userId, sessionId);

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var user = await _authService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            user.user_id,
            user.username,
            user.email,
            user.full_name,
            department = user.Department?.dept_name,
            departmentId = user.dept_id,
            role = user.Role?.role_name,
            roleId = user.role_id
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Password { get; set; }
    public int? DeptId { get; set; }
    public int? RoleId { get; set; }
}

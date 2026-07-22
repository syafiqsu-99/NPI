using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NPI.Server.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? Token, Users? User)> LoginAsync(string username, string password, string? ipAddress = null, string? userAgent = null);
        Task<(bool Success, string Message)> RegisterAsync(Users user, string password);
        Task<Users?> GetUserByIdAsync(int userId);
        string GenerateJwtToken(Users user, string sessionId);
        Task LogoutAsync(int userId, string? sessionId);
        Task TouchSessionAsync(int userId, string? sessionId);
        Task<List<SessionInfoDto>> GetActiveSessionsAsync(int userId, string? currentSessionId);
        Task<(bool Success, string Message)> RevokeSessionAsync(int userId, string sessionId, string reason);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool Success, string? Token, Users? User)> LoginAsync(string username, string password, string? ipAddress = null, string? userAgent = null)
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.username == username && u.is_active);

            if (user == null || !PasswordHelper.VerifyPassword(password, user.password_hash))
            {
                return (false, null, null);
            }

            user.last_login = DateTime.Now;

            var sessionId = Guid.NewGuid().ToString();
            var now = DateTime.Now;
            var session = new UserSessions
            {
                session_id = sessionId,
                user_id = user.user_id,
                token_hash = PasswordHelper.HashPassword(sessionId),
                ip_address = ipAddress,
                user_agent = userAgent,
                created_at = now,
                last_seen_at = now,
                expires_at = now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                is_active = true
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user, sessionId);
            return (true, token, user);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(Users user, string password)
        {
            if (await _context.Users.AnyAsync(u => u.username == user.username))
            {
                return (false, "Username already exists");
            }

            if (await _context.Users.AnyAsync(u => u.email == user.email))
            {
                return (false, "Email already exists");
            }

            user.password_hash = PasswordHelper.HashPassword(password);
            user.created_at = DateTime.Now;
            user.is_active = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "User registered successfully");
        }

        public async Task<Users?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.user_id == userId);
        }

        public string GenerateJwtToken(Users user, string sessionId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
                new Claim(ClaimTypes.Name, user.username),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Role, user.Role?.role_name ?? SystemRoles.Member),
                new Claim("DepartmentId", user.dept_id?.ToString() ?? "0"),
                new Claim("DepartmentName", user.Department?.dept_name ?? ""),
                new Claim("DepartmentCode", user.Department?.dept_code ?? ""),
                new Claim("FullName", user.full_name ?? ""),
                new Claim("SessionId", sessionId)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task LogoutAsync(int userId, string? sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return;

            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.user_id == userId && s.session_id == sessionId);

            if (session != null && session.is_active)
            {
                session.is_active = false;
                session.revoked_at = DateTime.Now;
                session.revoked_reason = "Logout";
                await _context.SaveChangesAsync();
            }
        }

        public async Task TouchSessionAsync(int userId, string? sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return;

            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.user_id == userId
                                       && s.session_id == sessionId
                                       && s.is_active);

            if (session == null)
                return;

            var now = DateTime.Now;
            if (session.last_seen_at.HasValue && (now - session.last_seen_at.Value).TotalMinutes < 1)
                return;

            session.last_seen_at = now;
            await _context.SaveChangesAsync();
        }

        public async Task<List<SessionInfoDto>> GetActiveSessionsAsync(int userId, string? currentSessionId)
        {
            var now = DateTime.Now;

            return await _context.UserSessions
                .Where(s => s.user_id == userId
                         && s.is_active
                         && s.revoked_at == null
                         && s.expires_at > now)
                .OrderByDescending(s => s.last_seen_at ?? s.created_at)
                .Select(s => new SessionInfoDto
                {
                    session_id = s.session_id,
                    ip_address = s.ip_address,
                    user_agent = s.user_agent,
                    created_at = s.created_at,
                    last_seen_at = s.last_seen_at,
                    expires_at = s.expires_at,
                    is_current = s.session_id == currentSessionId
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> RevokeSessionAsync(int userId, string sessionId, string reason)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.user_id == userId && s.session_id == sessionId);

            if (session == null)
                return (false, "Session not found.");

            if (!session.is_active || session.revoked_at != null)
                return (true, "Session already ended.");

            session.is_active = false;
            session.revoked_at = DateTime.Now;
            session.revoked_reason = reason;
            await _context.SaveChangesAsync();

            return (true, "Session revoked.");
        }
    }
}
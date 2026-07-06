using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NPI.Server.Data;
using NPI.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NPI.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Token, Users User)> LoginAsync(string username, string password)
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
            var session = new UserSessions
            {
                session_id = sessionId,
                user_id = user.user_id,
                token_hash = PasswordHelper.HashPassword(sessionId),
                created_at = DateTime.Now,
                expires_at = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                is_active = true
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
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

        public async Task<Users> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.user_id == userId);
        }

        public string GenerateJwtToken(Users user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
                new Claim(ClaimTypes.Name, user.username),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Role, user.Role?.role_name ?? "User"),
                new Claim("DepartmentId", user.dept_id?.ToString() ?? "0"),
                new Claim("DepartmentName", user.Department?.dept_name ?? ""),
                new Claim("FullName", user.full_name ?? "")
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

        public async Task LogoutAsync(int userId, string sessionId)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.user_id == userId && s.session_id == sessionId);

            if (session != null)
            {
                session.is_active = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}

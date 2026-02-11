using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Token, Users User)> LoginAsync(string username, string password);
        Task<(bool Success, string Message)> RegisterAsync(Users user, string password);
        Task<Users> GetUserByIdAsync(int userId);
        string GenerateJwtToken(Users user);
        Task LogoutAsync(int userId, string sessionId);
    }
}

using SurveyApp.Models;

namespace SurveyApp.Repositories.Interfaces
{
    public interface IAuthService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User> RegisterAsync(string username, string email, string password);
    }
}
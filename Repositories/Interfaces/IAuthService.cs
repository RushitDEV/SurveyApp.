using SurveyApp.Models;

namespace SurveyApp.Repositories.Interfaces
{
    public interface IAuthService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
        Task<User?> AuthenticateAsync(string UserName, string password);
        Task<User> RegisterAsync(string UserName, string email, string password);
    }
}
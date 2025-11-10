using SurveyApp.Models;
using SurveyApp.Repositories.Interfaces;

namespace SurveyApp.Repositories
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }

        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            if (await _userRepository.UsernameExistsAsync(username))
            {
                throw new Exception("Bu kullanıcı adı zaten kullanılıyor.");
            }

            if (await _userRepository.EmailExistsAsync(email))
            {
                throw new Exception("Bu e-posta adresi zaten kullanılıyor.");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            return await _userRepository.CreateAsync(user);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;
using SurveyApp.Models;
using SurveyApp.Repositories.Interfaces;

namespace SurveyApp.Repositories
{
    /// <summary>
    /// Identity ile birlikte kullanılan basitleştirilmiş User Repository
    /// UserManager, SignInManager zaten çoğu işi yapıyor
    /// Bu repository sadece özel sorgular için kullanılıyor
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm kullanıcıları anketleri ve yanıtlarıyla birlikte getirir
        /// </summary>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Surveys)
                .Include(u => u.Responses)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// ID'ye göre kullanıcı getirir
        /// </summary>
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Surveys)
                .Include(u => u.Responses)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        // ✅ Artık bu metodlara gerek yok, UserManager kullanıyoruz:
        // - CreateAsync → UserManager.CreateAsync()
        // - UpdateAsync → UserManager.UpdateAsync()
        // - DeleteAsync → UserManager.DeleteAsync()
        // - GetByUserNameAsync → UserManager.FindByNameAsync()
        // - GetByEmailAsync → UserManager.FindByEmailAsync()
        // - UserNameExistsAsync → UserManager.FindByNameAsync() != null
        // - EmailExistsAsync → UserManager.FindByEmailAsync() != null
    }
}
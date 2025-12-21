using SurveyApp.Models;

namespace SurveyApp.Repositories.Interfaces
{
    /// <summary>
    /// Identity ile birlikte kullanılacak basitleştirilmiş User Repository
    /// UserManager artık çoğu işlemi yapıyor (Create, Update, Delete, UserName/Email kontrolü)
    /// Bu interface sadece özel sorgular için kullanılacak
    /// </summary>
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetByIdAsync(int id);
    }
}
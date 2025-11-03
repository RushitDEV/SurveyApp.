using System.Linq.Expressions;

namespace SurveyApp.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Tüm kayıtları getir
        Task<IEnumerable<T>> GetAllAsync();

        // ID ile getir
        Task<T?> GetByIdAsync(int id);

        // Koşula göre getir (filtre)
        Task<IEnumerable<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);

        // Tek kayıt getir (koşula göre)
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        // Include ile ilişkili verileri getir
        Task<T?> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes);

        // Koşula göre include ile getir
        Task<IEnumerable<T>> GetWhereWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);

        // Ekle
        Task AddAsync(T entity);

        // Toplu ekle
        Task AddRangeAsync(IEnumerable<T> entities);

        // Güncelle
        void Update(T entity);

        // Toplu güncelle
        void UpdateRange(IEnumerable<T> entities);

        // Sil
        void Delete(T entity);

        // Toplu sil
        void DeleteRange(IEnumerable<T> entities);

        // ID ile sil
        Task DeleteByIdAsync(int id);

        // Var mı kontrolü
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        // Sayı getir
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}
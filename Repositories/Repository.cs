using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;
using SurveyApp.Repositories.Interfaces;
using System.Linq.Expressions;

namespace SurveyApp.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetWhereAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<T?> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task<IEnumerable<T>> GetWhereWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            foreach (var include in includes)
                query = query.Include(include);
            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);
        public void Update(T entity) => _dbSet.Update(entity);
        public void UpdateRange(IEnumerable<T> entities) => _dbSet.UpdateRange(entities);
        public void Delete(T entity) => _dbSet.Remove(entity);
        public void DeleteRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);
        public async Task DeleteByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null) _dbSet.Remove(entity);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
            => predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);
    }
}

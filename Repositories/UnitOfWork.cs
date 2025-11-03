using Microsoft.EntityFrameworkCore.Storage;
using SurveyApp.Data;
using SurveyApp.Models;
using SurveyApp.Repositories.Interfaces;
using System.Threading.Tasks;

namespace SurveyApp.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public IRepository<Survey> Surveys { get; }
        public IRepository<Question> Questions { get; }
        public IRepository<Option> Options { get; }
        public IRepository<Response> Responses { get; }
        public IRepository<Answer> Answers { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Surveys = new Repository<Survey>(_context);
            Questions = new Repository<Question>(_context);
            Options = new Repository<Option>(_context);
            Responses = new Repository<Response>(_context);
            Answers = new Repository<Answer>(_context);
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync() =>
            _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }
    }
}

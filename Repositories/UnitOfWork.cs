using Microsoft.EntityFrameworkCore.Storage;
using SurveyApp.Data;
using SurveyApp.Repositories.Interfaces;
using System.Threading.Tasks;

namespace SurveyApp.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;

        public ISurveyRepository Surveys { get; }
        public IQuestionRepository Questions { get; }
        public IOptionRepository Options { get; }
        public IResponseRepository Responses { get; }
        public IAnswerRepository Answers { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            ISurveyRepository surveyRepository,
            IQuestionRepository questionRepository,
            IOptionRepository optionRepository,
            IResponseRepository responseRepository,
            IAnswerRepository answerRepository)
        {
            _context = context;
            Surveys = surveyRepository;
            Questions = questionRepository;
            Options = optionRepository;
            Responses = responseRepository;
            Answers = answerRepository;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync() => _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync() => await _transaction.RollbackAsync();
    }
}

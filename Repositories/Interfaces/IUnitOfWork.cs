using SurveyApp.Models;

namespace SurveyApp.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Survey> Surveys { get; }
        IRepository<Question> Questions { get; }
        IRepository<Option> Options { get; }
        IRepository<Response> Responses { get; }
        IRepository<Answer> Answers { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

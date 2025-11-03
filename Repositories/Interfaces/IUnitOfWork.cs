using System.Threading.Tasks;

namespace SurveyApp.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        ISurveyRepository Surveys { get; }
        IQuestionRepository Questions { get; }
        IOptionRepository Options { get; }
        IResponseRepository Responses { get; }
        IAnswerRepository Answers { get; }

        // ✅ Transaction & Save methods
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

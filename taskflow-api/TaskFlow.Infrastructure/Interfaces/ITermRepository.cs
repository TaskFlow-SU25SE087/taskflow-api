using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ITermRepository
    {
        Task<Guid> GetTermIdAsync(string season, int year);
        Task CreateTermAsync(Term data);
        Task<DateTime?> GetLatestTermEndDateAsync();
        Task UpdateTermAsync(Term data);
        Task<Term?> GetTermByIdAsync(Guid termId);
        Task<List<Term>> GetAllTermsAsync(int page, int pageSize);
        Task<List<Term>> GetAllActiveTermsAsync();
        Task DeleteTermAsync(Guid termId);
        Task<Term?> GetCurrentTermAsync();
    }
}

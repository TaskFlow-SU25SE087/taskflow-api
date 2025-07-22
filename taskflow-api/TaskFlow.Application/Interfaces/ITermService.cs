using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ITermService
    {
        Task<List<Term>> GetListTerm(int page);
        Task CreateTerm(CreateTerm request);
        Task<Term?> GetTermById(Guid id);
        Task UpdateTerm(Guid termId, UpdateTerm request);
        Task LockTerm(Guid id);
        Task Delete(Guid id);
    }
}

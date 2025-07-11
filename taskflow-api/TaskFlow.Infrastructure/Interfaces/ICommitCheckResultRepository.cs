using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ICommitCheckResultRepository
    {
        Task CreateCommitResult(CommitCheckResult data);
    }
}

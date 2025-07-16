using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ICommitRecordRepository
    {
        Task Create(CommitRecord data);
        Task Update(CommitRecord data);
        Task<CommitRecord?> GetById(Guid commitRecordId);
        Task<bool> ExistsByCommitId(string commitId);
        Task<List<CommitRecordResponse>> GetCommitRecordsByPartId(Guid projectPartId, int page, int pageSize);
        Task<int> CountCommitByProjectPart(Guid projectPart);
    }
}

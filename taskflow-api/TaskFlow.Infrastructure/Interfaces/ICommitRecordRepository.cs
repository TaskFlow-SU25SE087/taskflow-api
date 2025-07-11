using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ICommitRecordRepository
    {
        Task Create(CommitRecord data);
        Task Update(CommitRecord data);
        Task<CommitRecord?> GetById(Guid commitRecordId);
        Task<bool> ExistsByCommitId(string commitId);
    }
}

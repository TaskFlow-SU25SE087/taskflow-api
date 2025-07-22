using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ICommitScanIssueRepository
    {
        Task<bool> CheckIssueReult(string message, string lineContent, string blamedEmail, string blamedName, string cleanFilePath);
        Task CreateAsync(CommitScanIssue issue);
        Task<List<CommitDetailResponse>> GetByCommitCheckResultId(string commitId);
        Task<List<CommitScanIssue>> GetByCommitCheckResultIdAsync(Guid commitRecordId);
    }
}

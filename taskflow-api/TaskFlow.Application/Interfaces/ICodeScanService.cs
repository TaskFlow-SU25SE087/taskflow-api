using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ICodeScanService
    {
        Task<CommitScanResult> ScanCommit(string extractPath, string projectKey);
        Task<List<SonarIssueResponse>> GetIssuesByProjectAsync(string projectKey);
    }
}

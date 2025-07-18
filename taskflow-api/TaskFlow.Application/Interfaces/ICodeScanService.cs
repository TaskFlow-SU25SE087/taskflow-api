using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ICodeScanService
    {
        Task<CommitScanResult> ScanCommit(string extractPath, string projectKey, ProgrammingLanguage language, Framework framework);
        Task<List<SonarIssueResponse>> GetIssuesByProjectAsync(string projectKey);
        Task<string> GetQualityGateStatusAsync(string projectKey);
        Task<ProjectMetricsDto> GetProjectMeasuresAsync(string projectKey);
    }
}

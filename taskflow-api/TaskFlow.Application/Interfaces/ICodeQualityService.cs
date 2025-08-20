using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ICodeQualityService
    {
        /// <summary>
        /// Analyzes a single C# file and returns quality metrics
        /// </summary>
        Task<CodeQualityReport> AnalyzeSingleFileAsync(IFormFile file, string userId);
        
        /// <summary>
        /// Analyzes an entire project (zip file) and returns comprehensive quality report
        /// </summary>
        Task<ProjectQualityReport> AnalyzeProjectAsync(IFormFile projectZip, string userId, string projectName);
        
        /// <summary>
        /// Analyzes a GitHub repository and returns quality report
        /// </summary>
        Task<ProjectQualityReport> AnalyzeGitHubRepoAsync(string repoUrl, string userId);
        
        /// <summary>
        /// Gets quality report history for a user
        /// </summary>
        Task<List<CodeQualityReport>> GetUserQualityHistoryAsync(string userId);
        
        /// <summary>
        /// Gets quality report history for a project
        /// </summary>
        Task<List<ProjectQualityReport>> GetProjectQualityHistoryAsync(string projectName);
        
        /// <summary>
        /// Gets a specific quality report by ID
        /// </summary>
        Task<CodeQualityReport?> GetQualityReportAsync(Guid reportId);
        
        /// <summary>
        /// Gets a specific project quality report by ID
        /// </summary>
        Task<ProjectQualityReport?> GetProjectQualityReportAsync(Guid reportId);
    }
}

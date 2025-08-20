using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class CodeQualityRequest
    {
        [Required]
        public string AnalysisType { get; set; } = "single"; // "single", "project", "github"
        
        public string? GitHubRepoUrl { get; set; }
        
        public string? ProjectName { get; set; }
        
        public string? Description { get; set; }
    }
}

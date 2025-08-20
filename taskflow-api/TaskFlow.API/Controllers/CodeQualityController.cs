using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CodeQualityController : ControllerBase
    {
        private readonly ICodeQualityService _codeQualityService;
        private readonly ILogger<CodeQualityController> _logger;

        public CodeQualityController(ICodeQualityService codeQualityService, ILogger<CodeQualityController> logger)
        {
            _codeQualityService = codeQualityService;
            _logger = logger;
        }

        /// <summary>
        /// Analyzes a single C# file for code quality
        /// </summary>
        /// <param name="file">The C# file to analyze</param>
        /// <returns>Code quality report with metrics and suggestions</returns>
        [HttpPost("analyze-file")]
        public async Task<ActionResult<CodeQualityReport>> AnalyzeSingleFile(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest("No file provided");

                if (!file.FileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only C# files (.cs) are supported");

                var userId = User.Identity?.Name ?? "anonymous";
                var report = await _codeQualityService.AnalyzeSingleFileAsync(file, userId);

                _logger.LogInformation("User {UserId} analyzed file {FileName} with quality score {Score}", 
                    userId, file.FileName, report.QualityScore);

                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing single file {FileName}", file?.FileName);
                return StatusCode(500, "An error occurred while analyzing the file");
            }
        }

        /// <summary>
        /// Analyzes an entire project (zip file) for code quality
        /// </summary>
        /// <param name="projectZip">ZIP file containing the project</param>
        /// <param name="request">Project analysis request details</param>
        /// <returns>Comprehensive project quality report</returns>
        [HttpPost("analyze-project")]
        public async Task<ActionResult<ProjectQualityReport>> AnalyzeProject(IFormFile projectZip, [FromForm] CodeQualityRequest request)
        {
            try
            {
                if (projectZip == null)
                    return BadRequest("No project file provided");

                if (!projectZip.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Please provide a ZIP file containing your project");

                if (string.IsNullOrWhiteSpace(request.ProjectName))
                    return BadRequest("Project name is required");

                var userId = User.Identity?.Name ?? "anonymous";
                var report = await _codeQualityService.AnalyzeProjectAsync(projectZip, userId, request.ProjectName);

                _logger.LogInformation("User {UserId} analyzed project {ProjectName} with overall quality score {Score}", 
                    userId, request.ProjectName, report.OverallQualityScore);

                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing project {ProjectName}", request?.ProjectName);
                return StatusCode(500, "An error occurred while analyzing the project");
            }
        }

        /// <summary>
        /// Analyzes a GitHub repository for code quality
        /// </summary>
        /// <param name="request">GitHub repository analysis request</param>
        /// <returns>Project quality report for the GitHub repository</returns>
        [HttpPost("analyze-github")]
        public async Task<ActionResult<ProjectQualityReport>> AnalyzeGitHubRepo([FromBody] CodeQualityRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.GitHubRepoUrl))
                    return BadRequest("GitHub repository URL is required");

                if (!request.GitHubRepoUrl.Contains("github.com"))
                    return BadRequest("Please provide a valid GitHub repository URL");

                var userId = User.Identity?.Name ?? "anonymous";
                var report = await _codeQualityService.AnalyzeGitHubRepoAsync(request.GitHubRepoUrl, userId);

                _logger.LogInformation("User {UserId} analyzed GitHub repo {RepoUrl} with overall quality score {Score}", 
                    userId, request.GitHubRepoUrl, report.OverallQualityScore);

                return Ok(report);
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "GitHub repository analysis is not yet implemented. Please use project upload instead.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing GitHub repository {RepoUrl}", request?.GitHubRepoUrl);
                return StatusCode(500, "An error occurred while analyzing the GitHub repository");
            }
        }

        /// <summary>
        /// Gets code quality history for the current user
        /// </summary>
        /// <returns>List of user's quality reports</returns>
        [HttpGet("my-history")]
        public async Task<ActionResult<List<CodeQualityReport>>> GetMyQualityHistory()
        {
            try
            {
                var userId = User.Identity?.Name ?? "anonymous";
                var reports = await _codeQualityService.GetUserQualityHistoryAsync(userId);

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quality history for user {UserId}", User.Identity?.Name);
                return StatusCode(500, "An error occurred while retrieving quality history");
            }
        }

        /// <summary>
        /// Gets quality history for a specific project
        /// </summary>
        /// <param name="projectName">Name of the project</param>
        /// <returns>List of project quality reports</returns>
        [HttpGet("project-history/{projectName}")]
        public async Task<ActionResult<List<ProjectQualityReport>>> GetProjectQualityHistory(string projectName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectName))
                    return BadRequest("Project name is required");

                var reports = await _codeQualityService.GetProjectQualityHistoryAsync(projectName);

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quality history for project {ProjectName}", projectName);
                return StatusCode(500, "An error occurred while retrieving project quality history");
            }
        }

        /// <summary>
        /// Gets a specific quality report by ID
        /// </summary>
        /// <param name="reportId">ID of the quality report</param>
        /// <returns>Quality report details</returns>
        [HttpGet("report/{reportId}")]
        public async Task<ActionResult<CodeQualityReport>> GetQualityReport(Guid reportId)
        {
            try
            {
                var report = await _codeQualityService.GetQualityReportAsync(reportId);
                
                if (report == null)
                    return NotFound("Quality report not found");

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quality report {ReportId}", reportId);
                return StatusCode(500, "An error occurred while retrieving the quality report");
            }
        }

        /// <summary>
        /// Gets a specific project quality report by ID
        /// </summary>
        /// <param name="reportId">ID of the project quality report</param>
        /// <returns>Project quality report details</returns>
        [HttpGet("project-report/{reportId}")]
        public async Task<ActionResult<ProjectQualityReport>> GetProjectQualityReport(Guid reportId)
        {
            try
            {
                var report = await _codeQualityService.GetProjectQualityReportAsync(reportId);
                
                if (report == null)
                    return NotFound("Project quality report not found");

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project quality report {ReportId}", reportId);
                return StatusCode(500, "An error occurred while retrieving the project quality report");
            }
        }

        /// <summary>
        /// Gets information about supported file types and analysis options
        /// </summary>
        /// <returns>Information about supported features</returns>
        [HttpGet("info")]
        [AllowAnonymous]
        public ActionResult<object> GetServiceInfo()
        {
            return Ok(new
            {
                ServiceName = "TaskFlow Code Quality Scanner",
                Description = "A student-friendly code quality analysis service for C# projects",
                SupportedFileTypes = new[] { ".cs" },
                SupportedAnalysisTypes = new[]
                {
                    new { Type = "single", Description = "Analyze a single C# file", MaxFileSize = "10MB" },
                    new { Type = "project", Description = "Analyze entire project (ZIP file)", MaxFileSize = "50MB" },
                    new { Type = "github", Description = "Analyze GitHub repository (Coming Soon)", MaxFileSize = "N/A" }
                },
                MetricsProvided = new[]
                {
                    "Code Quality Score (0-100)",
                    "Letter Grade (A, B, C, D, F)",
                    "Lines of Code Analysis",
                    "Cyclomatic Complexity",
                    "Code Duplication Detection",
                    "Method Length Analysis",
                    "Nesting Depth Analysis",
                    "Magic Number Detection"
                },
                Features = new[]
                {
                    "Student-friendly reports with clear explanations",
                    "Actionable suggestions for improvement",
                    "Visual quality indicators",
                    "Component-wise breakdown for projects",
                    "Quality trend tracking",
                    "Learning resources and examples"
                }
            });
        }
    }
}

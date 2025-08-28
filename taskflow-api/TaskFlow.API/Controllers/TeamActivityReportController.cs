using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeamActivityReportController : ControllerBase
    {
        private readonly ITeamActivityReportService _teamActivityReportService;
        private readonly ITaskFlowAuthorizationService _authorization;

        public TeamActivityReportController(
            ITeamActivityReportService teamActivityReportService,
            ITaskFlowAuthorizationService authorization)
        {
            _teamActivityReportService = teamActivityReportService;
            _authorization = authorization;
        }

        /// <summary>
        /// Generate a comprehensive team activity report for all members in a project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="request">Filter options for the report</param>
        /// <returns>Team activity report with individual member contributions</returns>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<ApiResponse<TeamActivityReportResponse>>> GetTeamActivityReport(
            Guid projectId,
            [FromQuery] TeamActivityReportRequest request)
        {
            try
            {
                // ✅ SECURITY: Input validation
                if (projectId == Guid.Empty)
                {
                    return BadRequest(ApiResponse<TeamActivityReportResponse>.Error(400, "Invalid project ID"));
                }

                // Verify user has access to this project (any role is fine for viewing reports)
                var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
                if (!isAuthorized)
                {
                    return BadRequest(ApiResponse<TeamActivityReportResponse>.Error(403, "You don't have permission to access this project's team activity report"));
                }

                var report = await _teamActivityReportService.GenerateTeamActivityReportAsync(projectId, request);
                return Ok(ApiResponse<TeamActivityReportResponse>.Success(report));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<TeamActivityReportResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TeamActivityReportResponse>.Error(500, "An error occurred while generating the team activity report"));
            }
        }

        /// <summary>
        /// Generate an activity report for a specific member in a project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="memberId">The member's user ID</param>
        /// <param name="request">Filter options for the report</param>
        /// <returns>Individual member activity report</returns>
        [HttpGet("project/{projectId}/member/{memberId}")]
        public async Task<ActionResult<ApiResponse<MemberActivityResponse>>> GetMemberActivityReport(
            Guid projectId,
            Guid memberId,
            [FromQuery] TeamActivityReportRequest request)
        {
            try
            {
                // ✅ SECURITY: Input validation
                if (projectId == Guid.Empty || memberId == Guid.Empty)
                {
                    return BadRequest(ApiResponse<MemberActivityResponse>.Error(400, "Invalid project or member ID"));
                }

                // Verify user has access to this project (any role is fine for viewing reports)
                var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
                if (!isAuthorized)
                {
                    return BadRequest(ApiResponse<MemberActivityResponse>.Error(403, "You don't have permission to access this project's team activity report"));
                }

                // Get current user ID to check if they're viewing their own report or if they're a leader
                var currentUserId = await _authorization.GetCurrentUserIdAsync();
                
                // Check if user is trying to view someone else's report
                if (memberId != currentUserId)
                {
                    // Only project leaders can view other members' reports
                    var isLeader = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
                    if (!isLeader)
                    {
                        return BadRequest(ApiResponse<MemberActivityResponse>.Error(403, "You can only view your own activity report. Project leaders can view all member reports."));
                    }
                }

                var report = await _teamActivityReportService.GenerateMemberActivityReportAsync(projectId, memberId, request);
                return Ok(ApiResponse<MemberActivityResponse>.Success(report));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<MemberActivityResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MemberActivityResponse>.Error(500, "An error occurred while generating the member activity report"));
            }
        }

        /// <summary>
        /// Generate a team activity report with POST method for complex filtering
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="request">Filter options for the report</param>
        /// <returns>Team activity report with individual member contributions</returns>
        [HttpPost("project/{projectId}")]
        public async Task<ActionResult<ApiResponse<TeamActivityReportResponse>>> GenerateTeamActivityReport(
            Guid projectId,
            [FromBody] TeamActivityReportRequest request)
        {
            try
            {
                // ✅ SECURITY: Input validation
                if (projectId == Guid.Empty)
                {
                    return BadRequest(ApiResponse<TeamActivityReportResponse>.Error(400, "Invalid project ID"));
                }

                // Verify user has access to this project (any role is fine for viewing reports)
                var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
                if (!isAuthorized)
                {
                    return BadRequest(ApiResponse<TeamActivityReportResponse>.Error(403, "You don't have permission to access this project's team activity report"));
                }

                var report = await _teamActivityReportService.GenerateTeamActivityReportAsync(projectId, request);
                return Ok(ApiResponse<TeamActivityReportResponse>.Success(report));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<TeamActivityReportResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TeamActivityReportResponse>.Error(500, "An error occurred while generating the team activity report"));
            }
        }

        /// <summary>
        /// Get burndown chart data for a specific sprint in a project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="sprintId">The sprint ID</param>
        /// <returns>Burndown chart data with effort points by priority and daily progress</returns>
        [HttpGet("project/{projectId}/sprint/{sprintId}/burndown-chart")]
        public async Task<ActionResult<ApiResponse<BurndownChartResponse>>> GetBurndownChart(
            Guid projectId,
            Guid sprintId)
        {
            try
            {
                // ✅ SECURITY: Input validation
                if (projectId == Guid.Empty || sprintId == Guid.Empty)
                {
                    return BadRequest(ApiResponse<BurndownChartResponse>.Error(400, "Invalid project or sprint ID"));
                }

                // ✅ SECURITY: Verify user has access to this project (any role is fine for viewing reports)
                var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
                if (!isAuthorized)
                {
                    return BadRequest(ApiResponse<BurndownChartResponse>.Error(403, "You don't have permission to access this project's burndown chart"));
                }

                var burndownChart = await _teamActivityReportService.GetBurndownChartAsync(projectId, sprintId);
                return Ok(ApiResponse<BurndownChartResponse>.Success(burndownChart));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<BurndownChartResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BurndownChartResponse>.Error(500, "An error occurred while generating the burndown chart"));
            }
        }
    }
}

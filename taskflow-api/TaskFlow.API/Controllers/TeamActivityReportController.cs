using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeamActivityReportController : ControllerBase
    {
        private readonly ITeamActivityReportService _teamActivityReportService;

        public TeamActivityReportController(ITeamActivityReportService teamActivityReportService)
        {
            _teamActivityReportService = teamActivityReportService;
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
    }
}

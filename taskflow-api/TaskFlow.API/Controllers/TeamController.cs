using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("admin/teams")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet("term/{termId}")]
        public async Task<ApiResponse<List<TeamResponse>>> GetTeamsByTerm(Guid termId)
        {
            var teams = await _teamService.GetTeamsByTermAsync(termId);
            return ApiResponse<List<TeamResponse>>.Success(teams);
        }

        [HttpGet("{projectId}")]
        public async Task<ApiResponse<TeamResponse?>> GetTeamById(Guid projectId)
        {
            var team = await _teamService.GetTeamByIdAsync(projectId);
            if (team == null)
            {
                return ApiResponse<TeamResponse?>.Error(404, "Team not found");
            }
            return ApiResponse<TeamResponse?>.Success(team);
        }

        [HttpGet]
        public async Task<ApiResponse<List<TeamResponse>>> GetAllTeams()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return ApiResponse<List<TeamResponse>>.Success(teams);
        }
    }
}

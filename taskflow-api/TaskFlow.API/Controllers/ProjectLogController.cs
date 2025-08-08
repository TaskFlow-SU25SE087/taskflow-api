using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Services;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("project/{projectId}/log")]
    [ApiController]
    public class ProjectLogController : ControllerBase
    {
        private readonly ITaskFlowAuthorizationService _authorization;
        private readonly ILogProjectService _logProjectService;

        public ProjectLogController(ITaskFlowAuthorizationService authorization, ILogProjectService logProjectService)
        {
            _authorization = authorization;
            _logProjectService = logProjectService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ApiResponse<List<ProjectLogResponse>>> GetProjectLogs([FromRoute] Guid projectId)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            var logs = await _logProjectService.AllLogPrj(projectId);
            return ApiResponse<List<ProjectLogResponse>>.Success(logs);
        }
    }
}

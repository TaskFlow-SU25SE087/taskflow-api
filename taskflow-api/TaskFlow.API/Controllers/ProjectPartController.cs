using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("projects/{projectId}/parts")]
    [ApiController]
    public class ProjectPartController : ControllerBase
    {
        private readonly ITaskFlowAuthorizationService _authorization;
        private readonly IProjectPartService _projectPartService;

        public ProjectPartController(ITaskFlowAuthorizationService authorization, IProjectPartService projectPartService)
        {
            _authorization = authorization;
            _projectPartService = projectPartService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ApiResponse<string>> CreateProjectPart(
            [FromRoute] Guid projectId, [FromBody] CreateProjectPartRequest request)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            await _projectPartService.CreatePart(projectId, request);
            return ApiResponse<string>.Success("Create information for project successfully");
        }

        [HttpPatch("{partId}/connect-repo")]
        [Authorize]
        public async Task<ApiResponse<string>> ConnectRepo(
            [FromRoute] Guid projectId, [FromRoute] Guid partId, [FromBody] ConnectRepoRequest request)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            await _projectPartService.ConnectRepo(partId, request);
            return ApiResponse<string>.Success("Connect repository successfully");
        }
    }
}

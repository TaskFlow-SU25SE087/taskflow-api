using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

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

        [HttpGet]
        [Authorize]
        public async Task<ApiResponse<List<ProjectPartResponse>>> GetAllRepositories([FromRoute] Guid projectId)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Member, ProjectRole.Leader);
            var parts = await _projectPartService.GetAllRepositories(projectId);
            return ApiResponse<List<ProjectPartResponse>>.Success(parts);
        }

        [HttpGet("{partId}/commits")]
        [Authorize]
        public async Task<ApiResponse<PagedResult<CommitRecordResponse>>> GetCommits(
            [FromRoute] Guid projectId, [FromRoute] Guid partId, int page)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Member, ProjectRole.Leader);
            var commits = await _projectPartService.GetCommits(partId, page);
            return ApiResponse<PagedResult<CommitRecordResponse>>.Success(commits);
        }

        [HttpGet("{partId}/commit/{commitId}")]
        [Authorize]
        public async Task<ApiResponse<List<CommitDetailResponse>>> GetCommit(
            [FromRoute] Guid projectId, [FromRoute] Guid partId, [FromRoute] string commitId)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Member, ProjectRole.Leader);
            var commit = await _projectPartService.GetCommitDetail(commitId);
            return ApiResponse<List<CommitDetailResponse>>.Success(commit);
        }

        [HttpDelete("webhook")]
        [Authorize]
        public async Task<ApiResponse<string>> DeleteWebhook([FromQuery] string repoUrl)
        {
            await _projectPartService.DeleteWeebhook(repoUrl);
            return ApiResponse<string>.Success("Delete webhook successfully");
        }
    }
}

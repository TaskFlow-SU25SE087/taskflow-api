using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("projects/{projectId}/parts/{projectPartId}/gitmember")]
    [ApiController]
    public class GitHubMemberController : ControllerBase
    {
        private readonly ITaskFlowAuthorizationService _authorization;
        private readonly IGitHubMemberService _gitHubMember;

        public GitHubMemberController(ITaskFlowAuthorizationService authorization, IGitHubMemberService gitHubMember)
        {
            _authorization = authorization;
            _gitHubMember = gitHubMember;
        }

        [HttpPost("{ProjectMemberId}")]
        [Authorize]
        public async Task<ApiResponse<string>> CreateGitMember(
            [FromRoute] Guid projectId, [FromRoute] Guid projectPartId,
            [FromRoute] Guid ProjectMemberId, [FromBody] CreateGitMemberRequest request)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            await _gitHubMember.CreateGitMember(projectPartId, ProjectMemberId, request);
            return ApiResponse<string>.Success("Create GitHub member successfully");
        }

        [HttpPatch("{Id}/local")]
        [Authorize]
        public async Task<ApiResponse<string>> AddGitLocal(
            [FromRoute] Guid projectId, [FromRoute] Guid projectPartId,
            [FromRoute] Guid Id, [FromBody] CreateGitMemberRequest request)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            await _gitHubMember.AddGitLocal(Id, request);
            return ApiResponse<string>.Success("Add GitHub member local successfully");
        }

        [HttpGet]
        [Authorize]
        public async Task<ApiResponse<List<GitMemberResponse>>> GetGitMember(
            [FromRoute] Guid projectId, [FromRoute] Guid projectPartId)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            var gitMembers = await _gitHubMember.GitMember(projectPartId);
            return ApiResponse<List<GitMemberResponse>>.Success(gitMembers);
        }
    }
}

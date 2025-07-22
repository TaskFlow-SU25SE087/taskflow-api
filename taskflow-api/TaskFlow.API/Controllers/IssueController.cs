using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using taskflow_api.Migrations;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Services;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("projects/{projectId}")]
    [ApiController]
    public class IssueController : ControllerBase
    {
        private readonly ITaskFlowAuthorizationService _authorization;
        private readonly IIssueService _issueService;

        public IssueController(ITaskFlowAuthorizationService authorization, IIssueService issueService)
        {
            _authorization = authorization;
            _issueService = issueService;
        }
        [HttpPost("tasks/{taskId}/issues/create")]
        public async Task<ApiResponse<string>> CreateIssueTask(
            [FromRoute] Guid projectId, [FromRoute] Guid taskId, [FromForm] CreateTaskIssueRequest request)
        {
            var projectmember = await _authorization
                .AuthorizeAndGetMemberAsync(projectId, ProjectRole.Leader, ProjectRole.Member);

            await _issueService.CreateTaskIssue(projectmember, projectId, taskId, request);
            return ApiResponse<string>.Success("Issue created successfully");
        }

        [HttpGet("issues")]
        public async Task<ApiResponse<List<IssueDetailResponse>>> GetAllIssue(
            [FromRoute] Guid projectId)
        {
            var projectmember = await _authorization
                .AuthorizeAndGetMemberAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            var issues = await _issueService.GetAllIssue(projectId);
            return ApiResponse<List<IssueDetailResponse>>.Success(issues);
        }
    }
}

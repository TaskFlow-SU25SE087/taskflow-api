using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("projects/{projectId}/sprints")]
    [ApiController]
    public class SprintController : ControllerBase
    {
        private readonly ISprintService _context;
        private readonly ITaskFlowAuthorizationService _authorization;
        public SprintController(ISprintService context, ITaskFlowAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        [HttpPost]
        [Authorize]
        public async Task<ApiResponse<bool>> CreateSprint([FromRoute] Guid projectId, CreateSprintRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.CreateSprint(projectId, request);
            return ApiResponse<bool>.Success(result);
        }

        [HttpPut("{sprintId}")]
        [Authorize]
        public async Task<ApiResponse<bool>> UpdateSprint(
            [FromRoute] Guid projectId, [FromRoute] Guid sprintId, UpdateSprintRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.UpdateSprint(projectId, sprintId, request);
            return ApiResponse<bool>.Success(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<ApiResponse<List<SprintResponse>>> GetListSprint([FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<List<SprintResponse>>.Error(9002, "Unauthorized access");
            }
            var result = await _context.ListPrints(projectId);
            return ApiResponse<List<SprintResponse>>.Success(result);
        }

        [HttpPost("{sprintId}/tasks/assign")]
        [Authorize]
        public async Task<ApiResponse<bool>> AddTasksToSprint(
            [FromRoute] Guid projectId, [FromRoute] Guid sprintId, [FromBody] List<Guid> taskIds)
        {
            var isAuthorized = await _authorization.AuthorizeAndGetMemberAsync(projectId, ProjectRole.Leader);

            await _context.AddTasksToSprint(projectId, sprintId, taskIds);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPost("{sprintId}/status")]
        [Authorize]
        public async Task<ApiResponse<bool>> ChangeStatusSprint(
            [FromRoute] Guid projectId, [FromRoute] Guid sprintId, [FromQuery] SprintStatus status)
        {
            var isAuthorized = await _authorization.AuthorizeAndGetMemberAsync(projectId, ProjectRole.Leader);
            await _context.ChangeStatusSprint(sprintId, status);
            return ApiResponse<bool>.Success(true);
        }

        [HttpGet("{sprintId}/tasks")]
        [Authorize]
        public async Task<ApiResponse<List<TaskProjectResponse>>> GetTasksInSprint(
            [FromRoute] Guid projectId, [FromRoute] Guid sprintId)
        {
             await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);

            var result = await _context.GetTaskInSprints(projectId, sprintId);
            return ApiResponse<List<TaskProjectResponse>>.Success(result);
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<ApiResponse<SprintResponse?>> GetCurrentSprint([FromRoute] Guid projectId)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            var result = await _context.GetCurrentSprint(projectId);
            return ApiResponse<SprintResponse?>.Success(result);
        }

        [HttpGet("{sprintId}/summary")]
        [Authorize]
        public async Task<ApiResponse<SprintSummaryReportResponse?>> GetSprintSummaryReport(
            [FromRoute] Guid projectId, [FromRoute] Guid sprintId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<SprintSummaryReportResponse?>.Error(9002, "Unauthorized access");
            }

            var result = await _context.GetSprintSummaryReport(projectId, sprintId);
            if (result == null)
            {
                return ApiResponse<SprintSummaryReportResponse?>.Error(404, "Sprint not found");
            }

            return ApiResponse<SprintSummaryReportResponse?>.Success(result);
        }
    }
}

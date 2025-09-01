using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("projects/{projectId}/tasks/{taskId}/assignments")]
    [ApiController]
    public class TaskAssignmentController : ControllerBase
    {
        private readonly ITaskFlowAuthorizationService _authorization;
        private readonly ITaskProjectService _context;

        public TaskAssignmentController(ITaskFlowAuthorizationService authorization, ITaskProjectService context)
        {
            _authorization = authorization;
            _context = context;
        }

        [HttpPost("accept")]
        [Authorize]
        public async Task<ApiResponse<bool>> UserAcceptTask([FromRoute] Guid taskId, [FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.userAcceptTask(projectId, taskId);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPost("assign")]
        [Authorize]
        public async Task<ApiResponse<bool>> AssignTaskToUser(
           [FromRoute] Guid taskId, [FromForm] AssignTaskRequest implement, [FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.AssignTaskToUser(taskId, projectId, implement);
            return ApiResponse<bool>.Success(true);
        }

        [HttpDelete("remove")]
        [Authorize]
        public async Task<ApiResponse<bool>> RevokeTaskAssignment(
            [FromRoute] Guid projectId, [FromRoute] Guid taskId, [FromBody] RemoveAssignmentReasonRequest reason)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.RevokeTaskAssignment(projectId, taskId, reason);
            return ApiResponse<bool>.Success(true);
        }

        [HttpDelete("leave")]
        [Authorize]
        public async Task<ApiResponse<bool>> LeaveTask(
            [FromRoute] Guid projectId, [FromRoute] Guid taskId, [FromBody] AssignmentReasonRequest reason)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.LeaveTask(projectId, taskId, reason);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPut("effort-points")]
        [Authorize]
        public async Task<ApiResponse<bool>> UpdateIndividualAssigneeEffortPoints(
            [FromRoute] Guid projectId, [FromRoute] Guid taskId, [FromBody] UpdateAssigneeEffortPointsRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.UpdateIndividualAssigneeEffortPoints(taskId, projectId, request);
            return ApiResponse<bool>.Success(true);
        }
    }
}

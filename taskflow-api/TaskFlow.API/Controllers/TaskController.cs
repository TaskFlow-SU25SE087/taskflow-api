using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using taskflow_api.TaskFlow.API.Hubs;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Services;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("project/{projectId}/task")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskProjectService _context;
        private readonly ITaskFlowAuthorizationService _authorization;
        private readonly ITaskCommentService _taskCommentService;
        
        private readonly IHubContext<TaskHub> _hubContext;

        public TaskController(ITaskProjectService context, ITaskFlowAuthorizationService authorization,
            ITaskCommentService taskCommentService, IHubContext<TaskHub> hubContext)
        {
            _context = context;
            _authorization = authorization;
            _taskCommentService = taskCommentService;
            _hubContext = hubContext;
        }

        [HttpGet("comment/add")]
        [Authorize]
        public async Task<ApiResponse<bool>> CreateComment(
            [FromBody]AddTaskCommentRequest request, [FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _taskCommentService.AddComentTask(request);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPost]
        [Authorize]
        public async Task<ApiResponse<TaskProject>> CreateTask(
            [FromBody] AddTaskRequest request, [FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<TaskProject>.Error(9002, "Unauthorized access");
            }
            var result = await _context.AddTask(request, projectId);
            return ApiResponse<TaskProject>.Success(result);
        }
        [HttpPut("delete/{taskId}")]
        [Authorize]
        public async Task<ApiResponse<bool>> DeleteTask(
            [FromRoute] Guid projectId,[FromRoute] Guid taskId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            bool result = await _context.DeleteTask(taskId);
            return ApiResponse<bool>.Success(result);
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<ApiResponse<TaskProject>> UpdateTask(
            [FromBody] UpdateTaskRequest request, [FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<TaskProject>.Error(9002, "Unauthorized access");
            }
            var result = await _context.UpdateTask(request);

            await _hubContext.Clients.Group(projectId.ToString())
                .SendAsync("TaskUpdated", new {
                    TaskId = result.Id,
                    Message = "Task has been updated",
                    Task = result
                });

            return ApiResponse<TaskProject>.Success(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<ApiResponse<List<TaskProjectResponse>>> GetAllTask([FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<List<TaskProjectResponse>>.Error(9002, "Unauthorized access");
            }

            var result = await _context.GetAllTask(projectId);
            return ApiResponse<List<TaskProjectResponse>>.Success(result);
        }

        [HttpPost("tags")]
        [Authorize]
        public async Task<ApiResponse<bool>> AddTagToTask(
            [FromRoute] Guid projectId, [FromQuery] Guid taskId, [FromQuery] Guid tagId)
        {
            await _context.AddTagForTask(projectId, taskId, tagId);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPost("accept")]
        [Authorize]
        public async Task<ApiResponse<bool>> UserAcceptTask([FromQuery] Guid taskId, [FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.userAcceptTask(taskId);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPost("assign")]
        [Authorize]
        public async Task<ApiResponse<bool>> AssignTaskToUser(
            [FromQuery] Guid taskId, [FromQuery] Guid assignerId, [FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.AssignTaskToUser(taskId, assignerId);
            return ApiResponse<bool>.Success(true);
        }

        [HttpDelete("leave/{taskAssigneeId}")]
        [Authorize]
        public async Task<ApiResponse<bool>> LeaveTask(
            [FromRoute] Guid projectId, [FromRoute] Guid taskAssigneeId, [FromQuery] string reason)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.LeaveTask(taskAssigneeId, reason);
            return ApiResponse<bool>.Success(true);
        }

        [HttpDelete("remove/{taskAssigneeId}")]
        [Authorize]
        public async Task<ApiResponse<bool>> RevokeTaskAssignment(
            [FromRoute] Guid projectId, [FromRoute] Guid taskAssigneeId, [FromQuery] string reason)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.RevokeTaskAssignment(taskAssigneeId, reason);
            return ApiResponse<bool>.Success(true);
        }
    }
}

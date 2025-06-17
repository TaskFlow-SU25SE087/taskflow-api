using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using taskflow_api.TaskFlow.API.Hubs;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Services;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("project/task")]
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
        public async Task<ApiResponse<bool>> CreateComment([FromBody]AddTaskCommentRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectID, ProjectRole.PM, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _taskCommentService.AddComentTask(request);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<ApiResponse<TaskProject>> CreateTask([FromBody] AddTaskRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId, ProjectRole.PM, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<TaskProject>.Error(9002, "Unauthorized access");
            }
            var result = await _context.AddTask(request);
            return ApiResponse<TaskProject>.Success(result);
        }
        [HttpPut("delete")]
        [Authorize]
        public async Task<ApiResponse<bool>> UpdateTask(Guid ProjectId, Guid TaskID)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(ProjectId, ProjectRole.PM, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            bool result = await _context.DeleteTask(TaskID);
            return ApiResponse<bool>.Success(result);
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<ApiResponse<TaskProject>> UpdateTask([FromBody] UpdateTaskRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId, ProjectRole.PM, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<TaskProject>.Error(9002, "Unauthorized access");
            }
            var result = await _context.UpdateTask(request);

            await _hubContext.Clients.Group(request.ProjectId.ToString())
                .SendAsync("TaskUpdated", new {
                    TaskId = result.Id,
                    Message = "Task has been updated",
                    Task = result
                });

            return ApiResponse<TaskProject>.Success(result);
        }
    }
}

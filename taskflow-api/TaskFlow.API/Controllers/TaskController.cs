﻿using Azure.Core;
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
    [Route("projects/{projectId}/tasks")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskProjectService _context;
        private readonly ITaskFlowAuthorizationService _authorization;
        private readonly ITaskCommentService _taskCommentService;
        
        

        public TaskController(ITaskProjectService context, ITaskFlowAuthorizationService authorization,
            ITaskCommentService taskCommentService, IHubContext<TaskHub> hubContext)
        {
            _context = context;
            _authorization = authorization;
            _taskCommentService = taskCommentService;
            
        }

        [HttpPost]
        [Authorize]
        public async Task<ApiResponse<bool>> CreateTask(
            [FromForm] AddTaskRequest request, [FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.AddTask(request, projectId);
            return ApiResponse < bool>.Success(true);
        }
        [HttpDelete("{taskId}")]
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

        [HttpPut("update/{taskId}")]
        [Authorize]
        public async Task<ApiResponse<TaskProject>> UpdateTask(
            [FromBody] UpdateTaskRequest request, [FromRoute] Guid projectId, [FromRoute] Guid taskId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<TaskProject>.Error(9002, "Unauthorized access");
            }
            var result = await _context.UpdateTask(request, taskId);

            

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

        [HttpPost("{taskId}/tags/{tagId}")]
        [Authorize]
        public async Task<ApiResponse<bool>> AddTagToTask(
            [FromRoute] Guid projectId, [FromRoute] Guid taskId, [FromRoute] Guid tagId)
        {
            await _context.AddTagForTask(taskId, tagId);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPost("{taskId}/upflie")]
        [Authorize]
        public async Task<ApiResponse<bool>> AcceptTask(
            [FromRoute] Guid projectId, [FromRoute] Guid taskId, [FromForm] CompleteTaskRequest data)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.SubmitTaskCompletion(projectId, taskId, data);
            return ApiResponse<bool>.Success(true);
        }

        [HttpGet("nounassigned-sprint")]
        [Authorize]
        public async Task<ApiResponse<List<ListTaskProjectNotSprint>>> GetAllTaskNotInSprint([FromRoute] Guid projectId)
        {
            await _authorization.AuthorizeAndGetMemberAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);

            var result = await _context.GettAllTaskNotSprint(projectId);
            return ApiResponse<List<ListTaskProjectNotSprint>>.Success(result);
        }

        [HttpPost("{taskId}/status/board/{boardId}")]
        [Authorize]
        public async Task<ApiResponse<bool>> ChangeBoard(
            [FromRoute] Guid boardId, [FromRoute] Guid taskId, [FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAndGetMemberAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            await _context.ChangeBoard(boardId, taskId);
            return ApiResponse<bool>.Success(true);
        }
    }
}

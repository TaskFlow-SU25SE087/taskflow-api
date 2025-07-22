using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Services;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("projects/{projectId}/tasks/{taskId}/comments")]
    [ApiController]
    public class TaskCommentController : ControllerBase
    {
        private readonly ITaskFlowAuthorizationService _authorization;
        private readonly ITaskProjectService _context;
        private readonly ITaskCommentService _taskCommentService;

        public TaskCommentController(ITaskFlowAuthorizationService authorization, ITaskProjectService context,
            ITaskCommentService taskCommentService)
        {
            _authorization = authorization;
            _context = context;
            _taskCommentService = taskCommentService;
        }

        [HttpPost("comment/add")]
        [Authorize]
        public async Task<ApiResponse<bool>> CreateComment(
            [FromForm] AddTaskCommentRequest request, [FromRoute] Guid projectId,[FromRoute] Guid taskId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _taskCommentService.AddComentTask(projectId, taskId, request);
            return ApiResponse<bool>.Success(true);
        }
    }
}

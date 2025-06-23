using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskTagController : ControllerBase
    {
        private readonly ITaskFlowAuthorizationService _authorization;
        private readonly ITaskProjectService _context;

        public TaskTagController(ITaskFlowAuthorizationService authorization, ITaskProjectService context)
        {
            _authorization = authorization;
            _context = context;
        }

        [HttpPost("tags")]
        [Authorize]
        public async Task<ApiResponse<bool>> AddTagToTask(
            [FromRoute] Guid projectId, [FromQuery] Guid taskId, [FromQuery] Guid tagId)
        {
            await _context.AddTagForTask(projectId, taskId, tagId);
            return ApiResponse<bool>.Success(true);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("projects/{projectId}/sprint-meetings")]
    [ApiController]
    public class SprintMeetingController : ControllerBase
    {
        private readonly ISprintMeetingLogsService _service;
        private readonly ITaskFlowAuthorizationService _authorization;

        public SprintMeetingController(ISprintMeetingLogsService service, ITaskFlowAuthorizationService authorization)
        {
            _service = service;
            _authorization = authorization;
        }

        [HttpGet]
        public async Task<ApiResponse<List<SprintMeetingResponse>>> GetAllSprintMeetings(Guid projectId)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            var meetings = await _service.GetAllSprintMetting(projectId);
            return ApiResponse<List<SprintMeetingResponse>>.Success(meetings);
        }

        [HttpGet("list-task-update")]
        public async Task<ApiResponse<List<UnfinishedTaskResponse>>> ListMyUpdatableUnfinished(Guid projectId)
        {
            var projectMemberId = await _authorization.AuthorizeAndGetMemberAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            var result = await _service.ListMyUpdatableUnfinished(projectId, projectMemberId, null);
            return ApiResponse<List<UnfinishedTaskResponse>>.Success(result);
        }

        [HttpPatch("{sprintmettingID}")]
        public async Task<ApiResponse<string>> UpdateResonTask([FromRoute]Guid sprintmettingID, [FromRoute] Guid projectId,
            Guid taskId, int itemVersion, string reason)
        {
            var projectMemberId = await _authorization.AuthorizeAndGetMemberAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            var result = await _service.UpdateResonTask(sprintmettingID, taskId, projectMemberId, itemVersion, reason);
            return ApiResponse<string>.Success(result);
        }

        [HttpPatch("{sprintmettingID}/next-plan")]
        public async Task<ApiResponse<bool>> UpdateNextPlan([FromRoute] Guid projectId, [FromRoute] Guid sprintmettingID, [FromBody] string nextPlan)
        {
            await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            await _service.UpdateNextPlan(sprintmettingID, nextPlan);
            return ApiResponse<bool>.Success(true);
        }
    }
}

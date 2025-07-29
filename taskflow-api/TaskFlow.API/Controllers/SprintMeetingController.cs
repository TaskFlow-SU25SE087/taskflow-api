using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;

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
    }
}

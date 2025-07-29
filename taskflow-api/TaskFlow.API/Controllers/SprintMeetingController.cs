using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("sprints/{sprintId}/meeting")]
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
    }
}

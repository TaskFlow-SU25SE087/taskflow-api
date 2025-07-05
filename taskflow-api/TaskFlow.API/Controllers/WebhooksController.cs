using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Services;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("api/webhooks")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly IProjectPartService _projectPartService;

        public WebhooksController(IProjectPartService projectPartService)
        {
            _projectPartService = projectPartService;
        }

        [HttpPost("github")]
        public async Task<ApiResponse<string>> GitHubWebhook([FromBody] JObject payload)
        {
            await _projectPartService.ProcessGitHubPushEvent(payload);
            return ApiResponse<string>.Success("succes");
        }
    }
}

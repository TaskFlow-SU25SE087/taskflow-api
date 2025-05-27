using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("project")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _context;
        private readonly ITaskFlowAuthorizationService _authorization;

        public ProjectController(IProjectService context, ITaskFlowAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        [HttpPost]
        [Authorize]
        public async Task<ApiResponse<ProjectResponse>> CreateProject([FromBody] CreateProjectRequest request)
        {
            var project = await _context.CreateProject(request);
            return ApiResponse<ProjectResponse>.Success(project);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using taskflow_api.Model.Common;
using taskflow_api.Model.Request;
using taskflow_api.Model.Response;
using taskflow_api.Service;

namespace taskflow_api.Controllers
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

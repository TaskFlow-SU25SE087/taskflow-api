using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("projects/{projectId}/sprints")]
    [ApiController]
    public class SprintController : ControllerBase
    {
        private readonly ISprintService _context;
        private readonly ITaskFlowAuthorizationService _authorization;
        public SprintController(ISprintService context, ITaskFlowAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        [HttpPost]
        [Authorize]
        public async Task<ApiResponse<bool>> CreateSprint([FromBody] CreateSprintRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.CreateSprint(request);
            return ApiResponse<bool>.Success(result);
        }

        [HttpPut("{sprintId}")]
        [Authorize]
        public async Task<ApiResponse<bool>> UpdateSprint([FromBody] UpdateSprintRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.UpdateSprint(request);
            return ApiResponse<bool>.Success(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<ApiResponse<List<Sprint>>> GetListSprint(Guid ProjectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(ProjectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<List<Sprint>>.Error(9002, "Unauthorized access");
            }
            var result = await _context.ListPrints(ProjectId);
            return ApiResponse<List<Sprint>>.Success(result);
        } 
    }
}

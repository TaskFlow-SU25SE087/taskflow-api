using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("project/member")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _context;
        private readonly ITaskFlowAuthorizationService _authorization;
        public MemberController(IMemberService context, ITaskFlowAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        [HttpPost("add")]
        [Authorize]
        public async Task<ApiResponse<bool>> AddMember([FromBody] AddMemberRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId, ProjectRole.PM);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var project = await _context.AddMember(request);
            return ApiResponse<bool>.Success(project);
        }

        [HttpPost("leave")]
        [Authorize]
        public async Task<ApiResponse<bool>> LeaveProject([FromQuery] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.PM, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.LeaveTheProject(projectId);
            return ApiResponse<bool>.Success(result);
        }

        [HttpDelete("remove")]
        [Authorize]
        public async Task<ApiResponse<bool>> RemoveMember([FromQuery] Guid projectId, [FromQuery] Guid userId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.PM);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.RemoveMember(projectId, userId);
            return ApiResponse<bool>.Success(result);
        }

        [HttpGet("verify-join")]
        public async Task<ApiResponse<bool>> VerifyJoinProject([FromQuery] string token)
        {
            bool result = await _context.VerifyJoinProject(token);
            return ApiResponse<bool>.Success(result);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("project/{projectId}/members")]
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
        public async Task<ApiResponse<bool>> AddMember([FromRoute] Guid projectId, [FromBody] AddMemberRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var project = await _context.AddMember(projectId, request);
            return ApiResponse<bool>.Success(project);
        }

        [HttpPost("leave")]
        [Authorize]
        public async Task<ApiResponse<bool>> LeaveProject([FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.LeaveTheProject(projectId);
            return ApiResponse<bool>.Success(result);
        }

        [HttpDelete("remove/{userId}")]
        [Authorize]
        public async Task<ApiResponse<bool>> RemoveMember([FromRoute] Guid projectId, [FromRoute] Guid userId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
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

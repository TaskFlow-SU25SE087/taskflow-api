using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("project/{projectId}/members")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _context;
        private readonly ITaskFlowAuthorizationService _authorization;
        private readonly AppSetting _appSetting;

        public MemberController(IMemberService context, ITaskFlowAuthorizationService authorization,
            IOptions<AppSetting> appSetting)
        {
            _context = context;
            _authorization = authorization;
            _appSetting = appSetting.Value;
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

        [HttpDelete("remove/{projectMemberId}")]
        [Authorize]
        public async Task<ApiResponse<bool>> RemoveMember([FromRoute] Guid projectId, [FromRoute] Guid projectMemberId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.RemoveMember(projectId, projectMemberId);
            return ApiResponse<bool>.Success(result);
        }

        [HttpGet("verify-join")]
        public async Task<IActionResult> VerifyJoinProjectRedirect([FromQuery] string token)
        {

            bool result = await _context.VerifyJoinProject(token);
            if (result)
            {
                return Redirect($"{_appSetting.FrontEndBaseUrl}/projects");
            }
            else
            {
                return Redirect($"{_appSetting.FrontEndBaseUrl}/auth/verify-falied");
            }
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<ApiResponse<List<MemberResponse>>> GetAllMemberInProject([FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<List<MemberResponse>>.Error(9002, "Unauthorized access");
            }
            var members = await _context.GetAllMemberInProject(projectId);
            return ApiResponse<List<MemberResponse>>.Success(members);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ApiResponse<ProjectMemberResponse>> GetMeInProject([FromRoute] Guid projectId)
        {
            Guid projectMemberId = await _authorization.AuthorizeAndGetMemberAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);

            var result = await _context.GetMeInProject(projectId, projectMemberId);
            return ApiResponse<ProjectMemberResponse>.Success(result);
        }

        [HttpPost("add-system-user")]
        [Authorize]
        public async Task<ApiResponse<bool>> AddSystemUser([FromRoute] Guid projectId)
        {
            await _context.AddSystemUSer(projectId);
            return ApiResponse<bool>.Success(true);
        }
    }
}

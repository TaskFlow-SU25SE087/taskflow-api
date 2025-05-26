using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using taskflow_api.Enums;
using taskflow_api.Model.Common;
using taskflow_api.Model.Request;
using taskflow_api.Model.Response;
using taskflow_api.Service;

namespace taskflow_api.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _context;
        private readonly ITaskFlowAuthorizationService _authorization;

        public UserController(IUserService context, ITaskFlowAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }
        [HttpPost("login")]
        public async Task<ApiResponse<string>> Login(LoginRequest model)
        {
            var token = await _context.Login(model);
            return ApiResponse<string>.Success(token);
        }

        [HttpPost]
        public async Task<ApiResponse<IdentityResult>> Register([FromForm] RegisterAccountRequest model)
        {
            return ApiResponse<IdentityResult>.Success(await _context.RegisterAccount(model));
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse<PagedResult<UserResponse>>> GetAllUser([FromQuery] PagingParams pagingParams)
        {
            var users = await _context.GetAllUser(pagingParams);
            return ApiResponse<PagedResult<UserResponse>>.Success(users);
        }

        [HttpPost("ban/{userId}")]
        [Authorize(Roles ="Admin")]
        public async Task<ApiResponse<UserResponse>> BanUser(Guid userId)
        {
            var user = await _context.BanUser(userId);
            return ApiResponse<UserResponse>.Success(user);
        }

        [HttpPost("unban/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse<UserResponse>> UnBanUser(Guid userId)
        {
            var user = await _context.UnBanUser(userId);
            return ApiResponse<UserResponse>.Success(user);
        }
    }
}

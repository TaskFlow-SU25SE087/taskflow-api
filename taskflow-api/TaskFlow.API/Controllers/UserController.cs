using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.API.Controllers
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
        public async Task<ApiResponse<TokenModel>> Login(LoginRequest model)
        {
            var token = await _context.Login(model);
            return ApiResponse<TokenModel>.Success(token);
        }

        [HttpPost]
        public async Task<ApiResponse<IdentityResult>> Register([FromForm] RegisterAccountRequest model)
        {
            return ApiResponse<IdentityResult>.Success(await _context.RegisterAccount(model));
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse<PagedResult<UserAdminResponse>>> GetAllUser([FromQuery] PagingParams pagingParams)
        {
            var users = await _context.GetAllUser(pagingParams);
            return ApiResponse<PagedResult<UserAdminResponse>>.Success(users);
        }

        [HttpPost("ban/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse<UserAdminResponse>> BanUser(Guid userId)
        {
            var user = await _context.BanUser(userId);
            return ApiResponse<UserAdminResponse>.Success(user);
        }

        [HttpPost("unban/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse<UserAdminResponse>> UnBanUser(Guid userId)
        {
            var user = await _context.UnBanUser(userId);
            return ApiResponse<UserAdminResponse>.Success(user);
        }

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<ApiResponse<UserResponse>> GetUserById(Guid userId)
        {
            var user = await _context.GetUserById(userId);
            return ApiResponse<UserResponse>.Success(user);
        }

        [HttpPut("{userId}")]
        [Authorize]
        public async Task<ApiResponse<UserResponse>> UpdateUser(Guid userId, [FromBody] UpdateUserRequest model)
        {
            var user = await _context.UpdateUser(userId, model);
            return ApiResponse<UserResponse>.Success(user);
        }

        [HttpPost("RenewToken")]
        public async Task<ApiResponse<TokenModel>> RenewToken(TokenModel model)
        {
            var newtoken = await _context.RenewToken(model);
            return ApiResponse<TokenModel>.Success(newtoken);
        }

    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Services;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<ApiResponse<TokenModel>> Login(LoginRequest model)
        {
            var token = await _userService.Login(model);
            return ApiResponse<TokenModel>.Success(token);
        }

        [HttpPost("register")]
        public async Task<ApiResponse<TokenModel>> Register([FromForm] RegisterAccountRequest model)
        {
            var result = await _userService.RegisterAccount(model);
            return ApiResponse<TokenModel>.Success(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ApiResponse<TokenModel>> RenewToken(TokenModel model)
        {
            var newToken = await _userService.RenewToken(model);
            return ApiResponse<TokenModel>.Success(newToken);
        }

        [HttpPost("verify-email")]
        [Authorize]
        public async Task<ApiResponse<bool>> VerifyAccount([FromQuery] string token)
        {
            var result = await _userService.VerifyAccount(token);
            return ApiResponse<bool>.Success(result);
        }

        [HttpPost("send-mail-again")]
        [Authorize]
        public async Task<ApiResponse<bool>> SendMailAgain()
        {
            await _userService.SendMailAgain();
            return ApiResponse<bool>.Success(true);
        }

        [HttpPost("add-username")]
        [Authorize]
        public async Task<ApiResponse<UserResponse>> AddUserName([FromForm] AddProfileUser model)
        {
            var result = await _userService.AddUserName(model);
            return ApiResponse<UserResponse>.Success(result);
        }
    }
}

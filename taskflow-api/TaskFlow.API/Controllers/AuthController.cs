using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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

        [HttpPost("token/refresh")]
        public async Task<ApiResponse<TokenModel>> RenewToken(TokenModel model)
        {
            var newToken = await _userService.RenewToken(model);
            return ApiResponse<TokenModel>.Success(newToken);
        }

        [HttpPost("email/verify")]
        [Authorize]
        public async Task<ApiResponse<bool>> VerifyAccount([FromQuery] string token)
        {
            var result = await _userService.VerifyAccount(token);
            return ApiResponse<bool>.Success(result);
        }

        [HttpPost("email/resend")]
        [Authorize]
        public async Task<ApiResponse<bool>> SendMailAgain()
        {
            await _userService.SendMailAgain();
            return ApiResponse<bool>.Success(true);
        }

        [HttpPost("username")]
        [Authorize]
        public async Task<ApiResponse<UserResponse>> AddUserName([FromForm] AddProfileUser model)
        {
            var result = await _userService.AddUserName(model);
            return ApiResponse<UserResponse>.Success(result);
        }

        [HttpPost("account/activate")]
        public async Task<ApiResponse<string>> ActivateAccount([FromBody] ActivateAccountRequest model)
        {
            await _userService.ConfirmEmailAndSetPasswordAsync(model);
            return ApiResponse<string>.Success("comfirm account successfully");
        }

        [HttpPost("account/reset-password")]
        public async Task<ApiResponse<string>> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            await _userService.ResetPassword(model);
            return ApiResponse<string>.Success("Password reset successfully");
        }

        [HttpPost("account/reset-password/send-mail")]
        public async Task<ApiResponse<string>> SendMailResetPassword([FromBody] string EmailOrUsername)
        {
            await _userService.SendMailResetPassword(EmailOrUsername);
            return ApiResponse<string>.Success("Reset password email sent successfully");
        }
    }
}

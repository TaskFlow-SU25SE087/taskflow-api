using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using taskflow_api.Enums;
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
        private readonly IAuthorizationService _authorization;

        public UserController(IUserService context, IAuthorizationService authorization)
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
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{userId}")]
        public async Task<ApiResponse<UserResponse>> GetUserById(Guid userId)
        {
            var user = await _userService.GetUserById(userId);
            return ApiResponse<UserResponse>.Success(user);
        }

        [HttpPut("{userId}")]
        public async Task<ApiResponse<UserResponse>> UpdateUser(Guid userId, [FromBody] UpdateUserRequest model)
        {
            var user = await _userService.UpdateUser(userId, model);
            return ApiResponse<UserResponse>.Success(user);
        }
    }
}

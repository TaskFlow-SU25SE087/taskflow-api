using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("admin/users")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class UserAdminController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserAdminController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ApiResponse<PagedResult<UserAdminResponse>>> GetAllUser([FromQuery] int page)
        {
            var users = await _userService.GetAllUser(page);
            return ApiResponse<PagedResult<UserAdminResponse>>.Success(users);
        }

        [HttpGet("term/{termId}")]
        public async Task<ApiResponse<PagedResult<UserAdminResponse>>> GetUsersByTerm([FromRoute] Guid termId, [FromQuery] int page = 1)
        {
            var users = await _userService.GetUsersByTerm(termId, page);
            return ApiResponse<PagedResult<UserAdminResponse>>.Success(users);
        }

        [HttpPatch("{userId}/ban")]
        public async Task<ApiResponse<UserAdminResponse>> BanUser(Guid userId)
        {
            var user = await _userService.BanUser(userId);
            return ApiResponse<UserAdminResponse>.Success(user);
        }

        [HttpPatch("{userId}/unban")]
        public async Task<ApiResponse<UserAdminResponse>> UnBanUser(Guid userId)
        {
            var user = await _userService.UnBanUser(userId);
            return ApiResponse<UserAdminResponse>.Success(user);
        }
        [HttpPost("import")]
        public async Task<ApiResponse<bool>> AddList([FromForm] ImportUserFileRequest file)
        {
            await _userService.ImportFileExcelAsync(file);
            return ApiResponse<bool>.Success(true);
        }

        [HttpGet("processing-files")]
        public async Task<ApiResponse<PagedResult<ProcessingFile>>> GetProcessingFiles([FromQuery] int page = 1)
        {
            var files = await _userService.getListFileProcess(page);
            return ApiResponse<PagedResult<ProcessingFile>>.Success(files);
        }

        [HttpPost("update-concurrency-stamp")]
        public async Task<ApiResponse<bool>> UpdateConcurrencyStamp()
        {
            await _userService.UpdateConcurrencyStamp();
            return ApiResponse<bool>.Success(true);
        }
    }
}

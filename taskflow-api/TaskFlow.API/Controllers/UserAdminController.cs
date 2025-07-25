﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;

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
            await _userService.ImportEnrollmentsFromExcelAsync(file);
            return ApiResponse<bool>.Success(true);
        }
    }
}

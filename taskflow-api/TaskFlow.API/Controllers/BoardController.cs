using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("project/board")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        private readonly IBoardService _context;
        private readonly ITaskFlowAuthorizationService _authorization;
        public BoardController(IBoardService context, ITaskFlowAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<ApiResponse<bool>> CreateBoard([FromBody] CreateBoardRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.CreateBoard(request);
            return ApiResponse<bool>.Success(result);
        }

        [HttpDelete("delete")]
        [Authorize]
        public async Task<ApiResponse<bool>> DeleteBoard([FromQuery] Guid boardId)
        {
            var IsAuthorized = await _authorization.AuthorizeAsync(boardId, ProjectRole.Leader);
            if (!IsAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.DeleteBoard(boardId);
            return ApiResponse<bool>.Success(result);
        }

        [HttpPut("board/order")]
        [Authorize]
        public async Task<ApiResponse<bool>> UpdateBoardOrder([FromBody] List<UpdateBoardRequest> request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request[0].ProjectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.UpdateBoardOrder(request);
            return ApiResponse<bool>.Success(result);
        }

        [HttpPut("board/update")]
        [Authorize]
        public async Task<ApiResponse<bool>> UpdateBoard([FromBody] UpdateBoardRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.UpdateBoard(request);
            return ApiResponse<bool>.Success(result);
        }

        [HttpGet("board/List")]
        [Authorize]
        public async Task<ApiResponse<List<BoardResponse>>> ListBoard(Guid ProjectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(ProjectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<List<BoardResponse>>.Error(9002, "Unauthorized access");
            }
            var result = await _context.ListBoardAsync(ProjectId);
            return ApiResponse<List<BoardResponse>>.Success(result);
        }
    }
}

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
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("projects/{projectId}/boards")]
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

        [HttpPost]
        [Authorize]
        public async Task<ApiResponse<bool>> CreateBoard([FromRoute] Guid projectId, [FromBody] CreateBoardRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.CreateBoard(projectId, request);
            return ApiResponse<bool>.Success(result);
        }

        [HttpDelete("{boardId}")]
        [Authorize]
        public async Task<ApiResponse<bool>> DeleteBoard([FromRoute] Guid projectId, [FromRoute] Guid boardId)
        {
            var IsAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            if (!IsAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.DeleteBoard(boardId);
            return ApiResponse<bool>.Success(result);
        }

        //change order of board
        [HttpPut("order")]
        [Authorize]
        public async Task<ApiResponse<bool>> UpdateBoardOrder(
            [FromRoute] Guid projectId, [FromBody] List<UpdateOrderBoardRequest> request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.UpdateBoardOrder(request);
            return ApiResponse<bool>.Success(result);
        }

        //update 1 board
        [HttpPut("{boardId}")]
        [Authorize]
        public async Task<ApiResponse<bool>> UpdateBoard(
            [FromRoute] Guid projectId, [FromRoute] Guid boardId, [FromBody] UpdateBoardRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            var result = await _context.UpdateBoard(projectId, boardId, request);
            return ApiResponse<bool>.Success(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<ApiResponse<List<BoardResponse>>> ListBoard([FromRoute] Guid projectId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<List<BoardResponse>>.Error(9002, "Unauthorized access");
            }
            var result = await _context.ListBoardAsync(projectId);
            return ApiResponse<List<BoardResponse>>.Success(result);
        }
    }
}

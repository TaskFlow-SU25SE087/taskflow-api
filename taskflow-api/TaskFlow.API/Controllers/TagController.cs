using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Services;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("project/Tag")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _context;
        private readonly ITaskFlowAuthorizationService _authorization;

        public TagController(ITagService Context, ITaskFlowAuthorizationService authorization)
        {
            _context = Context;
            _authorization = authorization;
        }

        [HttpPost]
        public async Task<ApiResponse<bool>> AddTag([FromBody] AddTagRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId,
                ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.AddTag(request);
            return ApiResponse<bool>.Success(true);
        }

        [HttpDelete("{TagId}")]
        public async Task<ApiResponse<bool>> DeleteTag(Guid TagId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(TagId,
                ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.DeleteTag(TagId);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPut]
        public async Task<ApiResponse<bool>> UpdateTag([FromBody] UpdateTagRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId,
                ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.UpdateTag(request);
            return ApiResponse<bool>.Success(true);

        }

        [HttpGet("getall")]
        public async Task<ApiResponse<List<TagResporn>>> getAllTag(Guid projectID)
        {
            var result = await _context.GetListTag(projectID);
            return ApiResponse<List<TagResporn>>.Success(result);
        }
    }
}

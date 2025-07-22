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
    [Route("projects/{projectId}/tags")]
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
        public async Task<ApiResponse<bool>> AddTag([FromRoute] Guid projectId, [FromBody] AddTagRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(
                projectId, ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.AddTag(projectId, request);
            return ApiResponse<bool>.Success(true);
        }

        [HttpDelete("{TagId}")]
        public async Task<ApiResponse<bool>> DeleteTag([FromRoute] Guid projectId, [FromRoute] Guid TagId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId,
                ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.DeleteTag(TagId);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPut("{tagId}")]
        public async Task<ApiResponse<bool>> UpdateTag(
            [FromRoute] Guid projectId, [FromRoute] Guid tagId, [FromBody] UpdateTagRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(projectId,
                ProjectRole.Leader, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _context.UpdateTag(projectId, tagId, request);
            return ApiResponse<bool>.Success(true);

        }

        [HttpGet]
        public async Task<ApiResponse<List<TagResporn>>> getAllTag([FromRoute]Guid projectId)
        {
            var result = await _context.GetListTag(projectId);
            return ApiResponse<List<TagResporn>>.Success(result);
        }
    }
}

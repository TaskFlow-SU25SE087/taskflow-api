using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Services;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("project/Tag")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _Tagservice;
        private readonly ITaskFlowAuthorizationService _authorization;

        public TagController(ITagService Tagservice, ITaskFlowAuthorizationService authorization)
        {
            _Tagservice = Tagservice;
            _authorization = authorization;
        }

        [HttpPost]
        public async Task<ApiResponse<bool>> AddTag([FromBody] AddTagRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId,
                ProjectRole.PM, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _Tagservice.AddTag(request);
            return ApiResponse<bool>.Success(true);
        }

        [HttpDelete("{TagId}")]
        public async Task<ApiResponse<bool>> DeleteTag(Guid TagId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(TagId,
                ProjectRole.PM, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _Tagservice.DeleteTag(TagId);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPut]
        public async Task<ApiResponse<bool>> UpdateTag([FromBody] UpdateTagRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId,
                ProjectRole.PM, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _Tagservice.UpdateTag(request);
            return ApiResponse<bool>.Success(true);

        }
    }
}

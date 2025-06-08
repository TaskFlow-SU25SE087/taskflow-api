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
    [Route("project/label")]
    [ApiController]
    public class LabelController : ControllerBase
    {
        private readonly ILabelService _labelService;
        private readonly ITaskFlowAuthorizationService _authorization;

        public LabelController(ILabelService labelService, ITaskFlowAuthorizationService authorization)
        {
            _labelService = labelService;
            _authorization = authorization;
        }

        [HttpPost]
        public async Task<ApiResponse<bool>> AddLabel([FromBody] AddLabelRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId,
                ProjectRole.PM, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _labelService.AddLabel(request);
            return ApiResponse<bool>.Success(true);
        }

        [HttpDelete("{labelId}")]
        public async Task<ApiResponse<bool>> DeleteLabel(Guid labelId)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(labelId,
                ProjectRole.PM, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _labelService.DeleteLabel(labelId);
            return ApiResponse<bool>.Success(true);
        }

        [HttpPut]
        public async Task<ApiResponse<bool>> UpdateLabel([FromBody] UpdateLabelRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId,
                ProjectRole.PM, ProjectRole.Member);
            if (!isAuthorized)
            {
                return ApiResponse<bool>.Error(9002, "Unauthorized access");
            }
            await _labelService.UpdateLabel(request);
            return ApiResponse<bool>.Success(true);

        }
    }
}

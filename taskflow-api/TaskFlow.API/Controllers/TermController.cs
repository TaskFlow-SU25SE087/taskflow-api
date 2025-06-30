using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Services;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("admin/term")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class TermController : ControllerBase
    {
        private readonly ITermService _termService;

        public TermController(ITermService termService)
        {
            _termService = termService;
        }

        [HttpGet("list")]
        public async Task<ApiResponse<List<Term>>> GetListTerm([FromQuery] int page)
        {
            var terms = await _termService.GetListTerm(page);
            return ApiResponse<List<Term>>.Success(terms);
        }

        [HttpPost]
        public async Task<ApiResponse<string>> CreateTerm(CreateTerm request)
        {
            await _termService.CreateTerm(request);
            return ApiResponse<string>.Success("create term succes");
        }

        [HttpPut("/{termId}")]
        public async Task<ApiResponse<string>> UpdateTerm([FromRoute] Guid termId, [FromQuery] UpdateTerm request)
        {
            await _termService.UpdateTerm(termId, request);
            return ApiResponse<string>.Success("update term succes");
        }

        [HttpDelete("lock/{termId}")]
        public async Task<ApiResponse<string>> LockTerm([FromRoute] Guid termId)
        {
            await _termService.LockTerm(termId);
            return ApiResponse<string>.Success("lock term succes");
        }
        [HttpDelete("/{termId}")]
        public async Task<ApiResponse<string>> DeleteTerm([FromRoute] Guid termId)
        {
            await _termService.Delete(termId);
            return ApiResponse<string>.Success("delete term succes");
        }

        [HttpGet("/{termId}")]
        public async Task<ApiResponse<Term>> GetTermById([FromRoute] Guid termId)
        {
            var term = await _termService.GetTermById(termId);
            return ApiResponse<Term>.Success(term!);
        }

    }
}

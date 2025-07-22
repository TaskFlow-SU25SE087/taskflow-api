using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("api/github")]
    [ApiController]
    public class GitHubController : ControllerBase
    {
        private readonly IGitHubRepoService _gitHubService;

        public GitHubController(IGitHubRepoService gitHubService)
        {
            _gitHubService = gitHubService;
        }

        [HttpGet("login-url")]
        public ApiResponse<string> GetGitHubLoginUrl()
        {
            var url = _gitHubService.GetGitHubLoginUrl();
            return ApiResponse<string>.Success(url);
        }

        [Authorize]
        [HttpPost("callback")]
        public async Task<ApiResponse<string>> ExchangeCodeForToken(string code)
        {
            var at = await _gitHubService.ExchangeCodeForToken(code);
            return ApiResponse<string>.Success(at);
        }

        [Authorize]
        [HttpGet("repos")]
        public async Task<ApiResponse<List<GitHubRepoDto>>> GetUserRepos()
        {
            var repos = await _gitHubService.GetUserRepos();
            return ApiResponse<List<GitHubRepoDto>>.Success(repos);
        }

        [Authorize]
        [HttpGet("connection-status")]
        public async Task<ApiResponse<bool>> CheckUserConnectGitHub()
        {
            var isConnected = await _gitHubService.CheckUserConnectGitHub();
            return ApiResponse<bool>.Success(isConnected);
        }
    }
}

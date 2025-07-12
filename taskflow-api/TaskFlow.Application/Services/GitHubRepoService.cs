using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class GitHubRepoService : IGitHubRepoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IUserGitHubRepository _tokenRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GitHubRepoService(HttpClient httpClient, IConfiguration config, 
            IUserGitHubRepository tokenRepo, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _config = config;
            _tokenRepo = tokenRepo;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("taskflow-app");
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> CheckUserConnectGitHub()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userIdStr = httpContext?.User.FindFirst("id")?.Value;
            if (userIdStr == null)
                throw new AppException(ErrorCode.Unauthorized);

            var userId = Guid.Parse(userIdStr);

            var token = await _tokenRepo.GetTokenByUserIdAsync(userId);
            return token != null;
        }

        public async Task<bool> CreateWebhook(string repoUrl, string token, string webhookUrl)
        {
            var uri = ConvertRepoUrlToApi(repoUrl) + "/hooks";

            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", token);
            request.Headers.UserAgent.ParseAdd("SEP-TaskFlow");

            var payload = new
            {
                name = "web",
                active = true,
                events = new[] { "push" },
                config = new
                {
                    url = webhookUrl,
                    content_type = "json"
                }
            };

            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<string> DownloadCommitSourceAsync(string repoFullName, string commitId, string accessToken)
        {
            var zipUrl = $"https://api.github.com/repos/{repoFullName}/zipball/{commitId}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", accessToken);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TaskFlow-Agent");

            var response = await _httpClient.GetAsync(zipUrl);
            response.EnsureSuccessStatusCode();

            var tempZipPath = Path.Combine(Path.GetTempPath(), $"{commitId}.zip");
            await using (var fs = new FileStream(tempZipPath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }
            //var extractPath = Path.Combine(Path.GetTempPath(), $"repo_{commitId}");
            var extractPath = Path.Combine(Path.GetTempPath(), $"{commitId}_{Guid.NewGuid()}");
            ZipFile.ExtractToDirectory(tempZipPath, extractPath, true);

            //delete file
            File.Delete(tempZipPath);
            return extractPath;
        }

        public async Task<string> ExchangeCodeForToken(string code)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;

            var clientId = _config["GitHub:ClientId"];
            var clientSecret = _config["GitHub:ClientSecret"];

            var requestData = new Dictionary<string, string>
            {
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"code", code}
            };

            var response = await _httpClient.PostAsync(
            "https://github.com/login/oauth/access_token",
            new FormUrlEncodedContent(requestData));

            var responseString = await response.Content.ReadAsStringAsync();
            var queryParams = System.Web.HttpUtility.ParseQueryString(responseString);
            var accessToken = queryParams["access_token"];

            //save access token to database
            var existingToken = await _tokenRepo.GetTokenByUserIdAsync(Guid.Parse(UserId!));
            if (existingToken != null)
            {
                existingToken.AccessToken = accessToken;
                existingToken.UpdateAt = DateTime.UtcNow;
                await _tokenRepo.Update(existingToken);
            }
            else
            {
                var newToken = new UserGitHubToken
                {
                    UserId = Guid.Parse(UserId!),
                    AccessToken = accessToken,
                    CreatedAt = DateTime.UtcNow,
                };
                await _tokenRepo.SaveTokenAsync(newToken);
            }
            return accessToken;
        }

        public string GetGitHubLoginUrl()
        {
            var clientId = _config["GitHub:ClientId"];
            var redirectUri = _config["GitHub:RedirectUri"];
            return $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope=repo";
        }

        public async Task<List<GitHubRepoDto>> GetUserRepos()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userIdStr = httpContext?.User.FindFirst("id")?.Value;
            if (userIdStr == null)
                throw new AppException(ErrorCode.Unauthorized);

            var userId = Guid.Parse(userIdStr);

            var tokenRecord = await _tokenRepo.GetTokenByUserIdAsync(userId);
            if (tokenRecord == null)
                throw new AppException(ErrorCode.GitHubTokenNotFound);
            var AT = tokenRecord.AccessToken;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", AT);

            var response = await _httpClient.GetAsync("https://api.github.com/user/repos");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var reposArray = JArray.Parse(content);

            var repos = reposArray.Select(x => new GitHubRepoDto
            {
                Name = x["name"]?.ToString() ?? "",
                FullName = x["full_name"]?.ToString() ?? "",
                HtmlUrl = x["html_url"]?.ToString() ?? ""
            }).ToList();

            return repos;
        }

        public async Task<bool> TestConnection(string repoUrl, string token)
        {
            //convert url to github
            var uri = ConvertRepoUrlToApi(repoUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            // set the user agent
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", token);
            request.Headers.UserAgent.ParseAdd("SEP-TaskFlow");

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        private string ConvertRepoUrlToApi(string repoUrl)
        {
            var segments = new Uri(repoUrl).Segments;
            var owner = segments[1].TrimEnd('/');
            var repo = segments[2].TrimEnd('/');
            return $"https://api.github.com/repos/{owner}/{repo}";
        }
    }
}

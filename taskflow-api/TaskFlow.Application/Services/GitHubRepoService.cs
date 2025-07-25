﻿using Azure;
using Azure.Core;
    using Microsoft.AspNetCore.Http.HttpResults;
    using Newtonsoft.Json.Linq;
using System.Diagnostics;
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
        private readonly ILogger<GitHubRepoService> _logger;

        public GitHubRepoService(HttpClient httpClient, IConfiguration config, 
                IUserGitHubRepository tokenRepo, IHttpContextAccessor httpContextAccessor,
                ILogger<GitHubRepoService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _tokenRepo = tokenRepo;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("taskflow-app");
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

        public async Task<string> CloneRepoAndCheckoutAsync(string repoFullName, string commitId, string accessToken)
        {
            var tmpPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tmpPath);

            var repoUrl = $"https://{accessToken}@github.com/{repoFullName}.git";
            async Task RunProcess(string fileName, string arguments, string workingDir)
            {
                //clone repo code git 
                var process = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                _logger.LogInformation("Running command: {File} {Args} in {Dir}", fileName, arguments, workingDir);
                var proc = Process.Start(process)!;
                var output = await proc.StandardOutput.ReadToEndAsync();
                var error = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();

                _logger.LogInformation("Command output: {Output}", output);
                if (!string.IsNullOrWhiteSpace(error))
                    _logger.LogError("Command error: {Error}", error);
                _logger.LogInformation("Command exit code: {ExitCode}", proc.ExitCode);


                if (proc.ExitCode != 0)
                    throw new Exception($"Command '{fileName} {arguments}' failed. Check server logs.");

                //clone repo code git
                await RunProcess("git", $"clone {repoUrl} .", tmpPath);
                //checkout commit
                await RunProcess("git", $"checkout {commitId}", tmpPath);
            }

            return tmpPath;
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
                var extractPath = Path.Combine(Path.GetTempPath(), $"{commitId}_{Guid.NewGuid()}");

                var cloneUrl = $"https://{accessToken}:x-oauth-basic@github.com/{repoFullName}.git";

                //clone .git and repo git 
                var cloneProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"clone \"{cloneUrl}\" \"{extractPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                cloneProcess.Start();
                string cloneOutput = await cloneProcess.StandardOutput.ReadToEndAsync();
                string cloneError = await cloneProcess.StandardError.ReadToEndAsync();
                await cloneProcess.WaitForExitAsync();

                if (cloneProcess.ExitCode != 0)
                    throw new Exception($"git clone failed:\n{cloneError}");

                 //checkout commit
                 var checkoutProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"checkout {commitId}",
                        WorkingDirectory = extractPath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                checkoutProcess.Start();
                string checkoutOutput = await checkoutProcess.StandardOutput.ReadToEndAsync();
                string checkoutError = await checkoutProcess.StandardError.ReadToEndAsync();
                await checkoutProcess.WaitForExitAsync();

                if (checkoutProcess.ExitCode != 0)
                    throw new Exception($"git checkout failed:\n{checkoutError}");

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
            return $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope=repo,admin:repo_hook,user:email";
            }

        public async Task<List<MemberInGithubResponse>> GetGitHubRepoMembers(string repoUrl, string accessToken)
        {
            var uri = ConvertRepoUrlToApi(repoUrl) + "/collaborators";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", accessToken);
            request.Headers.UserAgent.ParseAdd("SEP-TaskFlow");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var membersArray = JArray.Parse(content);

            var members = new List<MemberInGithubResponse>();
            foreach (var x in membersArray)
            {
                var login = x["login"]?.ToString() ?? "";
                var avatarUrl = x["avatar_url"]?.ToString() ?? "";

                string email = "";
                string displayName = login;

                try
                {
                    var userDetailRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/users/{login}");
                    userDetailRequest.Headers.Authorization = new AuthenticationHeaderValue("token", accessToken);
                    userDetailRequest.Headers.UserAgent.ParseAdd("SEP-TaskFlow");

                    var userDetailResponse = await _httpClient.SendAsync(userDetailRequest);
                    if (userDetailResponse.IsSuccessStatusCode)
                    {
                        var userContent = await userDetailResponse.Content.ReadAsStringAsync();
                        var userJson = JObject.Parse(userContent);

                        email = userJson["email"]?.ToString() ?? "";

                        var rawName = userJson["name"];
                        displayName = (rawName != null && !string.IsNullOrWhiteSpace(rawName.ToString()))
                            ? rawName.ToString()!
                            : login;
                    }
                }
                catch
                {
                }

                members.Add(new MemberInGithubResponse
                {
                    Name = displayName,
                    Email = email,
                    AvatarUrl = avatarUrl
                });

                await Task.Delay(200);
            }
            return members;
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

using Azure;
using System.Net.Http;
using System.Text.Json;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class GitHubRepoService : IRepoService
    {
        private readonly HttpClient _httpClient;

        public GitHubRepoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<bool> CreateWebhook(string repoUrl, string token, string webhookUrl)
        {
            var uri = ConvertRepoUrlToApi(repoUrl) + "/hooks";

            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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

        public async Task<bool> TestConnection(string repoUrl, string token)
        {
            //convert url to github
            var uri = ConvertRepoUrlToApi(repoUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            // set the user agent
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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

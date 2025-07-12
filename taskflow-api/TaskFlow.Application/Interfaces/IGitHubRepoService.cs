using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IGitHubRepoService
    {
        Task<bool> TestConnection(string repoUrl, string token);
        Task<bool> CreateWebhook(string repoUrl, string token, string webhookUrl);
        Task<string> DownloadCommitSourceAsync(string repoFullName, string commitId, string accessToken);
        string GetGitHubLoginUrl();
        Task<string> ExchangeCodeForToken(string code);
        Task<List<GitHubRepoDto>> GetUserRepos();
        Task<bool> CheckUserConnectGitHub();
    }
}

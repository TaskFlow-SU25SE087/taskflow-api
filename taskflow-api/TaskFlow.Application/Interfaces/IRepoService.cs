namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IRepoService
    {
        Task<bool> TestConnection(string repoUrl, string token);
        Task<bool> CreateWebhook(string repoUrl, string token, string webhookUrl);
    }
}

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class ConnectRepoRequest
    {
        public string RepoUrl { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
    }
}

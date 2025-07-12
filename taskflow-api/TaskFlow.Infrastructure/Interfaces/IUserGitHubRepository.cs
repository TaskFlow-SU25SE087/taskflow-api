using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IUserGitHubRepository
    {
        Task SaveTokenAsync(UserGitHubToken token);
        Task<UserGitHubToken?> GetTokenByUserIdAsync(Guid userId);
    }
}

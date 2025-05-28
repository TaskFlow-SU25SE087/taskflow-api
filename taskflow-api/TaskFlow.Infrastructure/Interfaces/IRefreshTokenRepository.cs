using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task CreateRefreshTokenAsync(RefeshToken model);
        Task<RefeshToken?> GetRefreshTokenByToken(string token);
        Task UpdateRefreshTokenAsync(RefeshToken model);
    }
}

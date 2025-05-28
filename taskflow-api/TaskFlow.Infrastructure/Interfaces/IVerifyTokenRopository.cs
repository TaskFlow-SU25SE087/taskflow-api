using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IVerifyTokenRopository
    {
        Task AddVerifyTokenAsync(VerifyToken data);
        Task<VerifyToken?> GetVerifyTokenAsync(string token);
        Task UpdateTokenAsync(VerifyToken data);
        Task<VerifyToken?> GetVerifyTokenByUserIdAndType(Guid id, VerifyTokenEnum Type);
    }
}

using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IProjectPartRepository
    {
        Task CreatePartAsync(ProjectPart data);
        Task<ProjectPart?> GetPartByIdAsync(Guid partId);
        Task UpdateAsync(ProjectPart part);
        Task<ProjectPart?> GetByRepoUrlAsync(string repoUrl);
    }
}

using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IProjectRepository
    {
        Task<Guid> CreateProjectAsync(string title);
        Task<Project?> GetProjectByIdAsync(Guid id);
        Task UpdateProject(Project data);

    }
}

using System.Threading.Tasks;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IProjectRepository
    {
        Task<Guid> CreateProjectAsync(Project data);
        Task<Project?> GetProjectByIdAsync(Guid id);
        Task UpdateProject(Project data);
        IQueryable<Project> GetProjectsByUserIdAsync(Guid userId);
        Task<List<ProjectsResponse>> GetListProjectResponseByUserAsync(Guid userId);
        Task<List<ProjectsResponse>> GetAllProjectsAsync();
        Task<List<ProjectsResponse>> GetProjectsByTermAsync(Guid termId);
        Task<bool> DeleteProjectAsync(Guid projectId);
    }
}

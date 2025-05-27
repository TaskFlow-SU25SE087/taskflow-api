using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IProjectService
    {
        public Task<ProjectResponse> CreateProject(CreateProjectRequest request);
    }
}

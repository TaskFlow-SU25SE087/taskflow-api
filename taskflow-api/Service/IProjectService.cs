using taskflow_api.Model.Request;
using taskflow_api.Model.Response;

namespace taskflow_api.Service
{
    public interface IProjectService
    {
        public Task<ProjectResponse> CreateProject(CreateProjectRequest request);
    }
}

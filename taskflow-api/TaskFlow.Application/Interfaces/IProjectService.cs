using Microsoft.AspNetCore.Mvc.RazorPages;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IProjectService
    {
         Task<ProjectResponse> CreateProject(CreateProjectRequest request);
         Task<ProjectResponse> UpdateProject(UpdateProjectRequest request);
        Task<ProjectDetailResponse> GetProject(Guid ProjectId);
        Task<List<ProjectsResponse>> ListProjectResponse();
        Task<bool> DeleteProject(Guid projectId);
    }
}

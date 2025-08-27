using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ILogProjectService
    {
        Task<List<ProjectLogResponse>> AllLogPrj(Guid projectId);
        Task LogCreateProject(Guid projectId, Guid projectMemberId);
        Task LogDeleteProject(Guid projectId, Guid projectMemberId);
        Task LogJoinProject(Guid projectId, Guid projectMemberId);
    }
}

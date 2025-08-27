using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ILogProjectService
    {
        Task<List<ProjectLogResponse>> AllLogPrj(Guid projectId);
        Task LogAddTaskToSprint(Guid actorMemberId, Guid sprintId, List<TaskProject> tasks);
        Task LogCreateProject(Guid projectId, Guid projectMemberId);
        Task LogCreateSprint(Guid sprintId);
        Task LogDeleteProject(Guid projectId, Guid projectMemberId);
        Task LogJoinProject(Guid projectId, Guid projectMemberId);
        Task LogLeaveProject(Guid projectId, Guid projectMemberId);
        Task LogRemoveMember(Guid projectId, Guid id, Guid actorMemberId);
        Task UpdateTitleSprint(Guid sprintId, Guid actorMemberId, ChangedField field, string oldValue, string newValue);
    }
}

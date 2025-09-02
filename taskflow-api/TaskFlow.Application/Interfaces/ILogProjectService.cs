using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ILogProjectService
    {
        Task<PagedResult<ProjectLogResponse>> AllLogPrj(Guid projectId, Guid? nextLogId);
        Task LogAddTaskToSprint(Guid actorMemberId, Guid sprintId, List<TaskProject> tasks);
        Task LogCreateProject(Guid projectId, Guid projectMemberId);
        Task LogCreateSprint(Guid sprintId);
        Task LogDeleteProject(Guid projectId, Guid projectMemberId);
        Task LogDeleteSprint(Guid sprintId);
        Task LogJoinProject(Guid projectId, Guid projectMemberId);
        Task LogLeaveProject(Guid projectId, Guid projectMemberId);
        Task LogRemoveMember(Guid projectId, Guid id, Guid actorMemberId);
        Task UpdateTitleSprint(Guid sprintId, Guid actorMemberId, ChangedField field, string oldValue, string newValue);
        Task CreateRessonTaskLog(Guid projectId, Guid taskId, Guid actorMemberId, string reason);
    }
}

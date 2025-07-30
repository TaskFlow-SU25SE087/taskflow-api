using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ISprintMeetingLogsService
    {
        Task CreateSprintMetting(Guid SprintId);
        Task<List<SprintMeetingResponse>> GetAllSprintMetting(Guid projectId);
        Task<List<UnfinishedTaskResponse>> ListMyUpdatableUnfinished(Guid projectId, Guid projectMemberId, Guid? nextCursor);
        Task<string> UpdateResonTask(Guid mettingID, Guid taskId, Guid projectMemberId, int itemVersion, string reason);
        Task UpdateNextPlan(Guid mettingID, string nextPlan);
    }
}

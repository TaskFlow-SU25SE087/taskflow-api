using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ISprintMeetingLogsService
    {
        Task CreateSprintMetting(Guid SprintId);
        Task<List<SprintMeetingResponse>> GetAllSprintMetting(Guid projectId);
        Task<Object> ListMyUpdatableUnfinished(Guid projectId, Guid projectMemberId, Guid? nextCursor);
    }
}

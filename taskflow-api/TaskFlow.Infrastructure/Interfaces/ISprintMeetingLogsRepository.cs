using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ISprintMeetingLogsRepository
    {
        Task CreateMetting(SprintMeetingLog data);
        Task<List<SprintMeetingResponse>> GetAllSprintMetting(Guid projectId);
        Task<List<SprintMeetingLog>> GetAllSprintMettingCanUpdate(Guid projectId, DateTime since);
    }
}

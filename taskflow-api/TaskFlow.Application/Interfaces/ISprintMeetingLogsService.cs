using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ISprintMeetingLogsService
    {
        Task CreateSprintMetting(Guid SprintId);
    }
}

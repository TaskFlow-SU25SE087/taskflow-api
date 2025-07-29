using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ISprintRepository
    {
        Task CreateSprintAsync(Sprint sprint);
        Task UpdateSprintAsync(Sprint sprint);
        Task<List<SprintResponse>> GetListPrintAsync(Guid projectId);
        Task<Sprint?> GetSprintByIdAsync(Guid sprintId);
        Task<bool> CheckSprintName(Guid projectId, string name);
        Task <Sprint?> GetNextSprint(Guid projectId, DateTime afterDate);
        Task<bool> CheckSprintStartDate(Guid projectId);
        Task<SprintResponse?> GetCurrentSprint(Guid projectId);
        Task<List<TaskCompleteDTO>> GetTaskCompletes(Guid sprintId, Guid projectId);
        Task<List<UnfinishedTaskDto>> GetUnFinishTasks(Guid sprintId, Guid projectId);
    }
}

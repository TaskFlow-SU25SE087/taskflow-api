using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ISprintService
    {
        public Task<bool> CreateSprint(Guid ProjectId, CreateSprintRequest request);
        public Task<bool> UpdateSprint(Guid ProjectId, Guid SprintId, UpdateSprintRequest request);
        public Task<List<SprintResponse>> ListPrints(Guid ProjectId);
        Task AddTasksToSprint(Guid ProjectId, Guid SprintId, List<Guid> TaskID);
        Task ChangeStatusSprint(Guid SpringId, SprintStatus status);
        Task<List<TaskProjectResponse>> GetTaskInSprints(Guid ProjectId, Guid SprintId);
        Task<SprintResponse?> GetCurrentSprint(Guid ProjectId);
    }
}

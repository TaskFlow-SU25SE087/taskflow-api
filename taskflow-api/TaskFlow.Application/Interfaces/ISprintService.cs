using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ISprintService
    {
        public Task<bool> CreateSprint(Guid ProjectId, CreateSprintRequest request);
        public Task<bool> UpdateSprint(Guid ProjectId, Guid actorMemberId, Guid SprintId, UpdateSprintRequest request);
        public Task<bool> DeleteSprint(Guid ProjectId, Guid SprintId);
        public Task<List<SprintResponse>> ListPrints(Guid ProjectId);
        Task AddTasksToSprint(Guid ProjectId, Guid SprintId, List<Guid> TaskID, Guid memberId);
        Task ChangeStatusSprint(Guid SpringId, SprintStatus status);
        Task<List<TaskProjectResponse>> GetTaskInSprints(Guid ProjectId, Guid SprintId);
        Task<SprintResponse?> GetCurrentSprint(Guid ProjectId);
        Task<SprintSummaryReportResponse?> GetSprintSummaryReport(Guid ProjectId, Guid SprintId);
    }
}

using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ISprintService
    {
        public Task<bool> CreateSprint(CreateSprintRequest request);
        public Task<bool> UpdateSprint(UpdateSprintRequest request);
        public Task<List<Sprint>> ListPrints(Guid ProjectId);
    }
}

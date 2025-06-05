using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ISprintService
    {
        public Task<bool> CreateSprint(CreateSprintRequest request);
        public Task<bool> UpdateSprint(UpdateSprintRequest request);
    }
}

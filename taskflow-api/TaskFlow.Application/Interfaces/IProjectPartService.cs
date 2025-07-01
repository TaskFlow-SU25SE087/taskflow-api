using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IProjectPartService
    {
        Task CreatePart(Guid ProjectId, CreateProjectPartRequest request);
    }
}

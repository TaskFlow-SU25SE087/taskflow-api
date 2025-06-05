using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ISprintRepository
    {
        Task CreateSprintAsync(Sprint sprint);
        Task UpdateSprintAsync(Sprint sprint);
    }
}

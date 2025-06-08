using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ILabelRepository
    {
        Task AddLabelAsync(Labels label);
    }
}

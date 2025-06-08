using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ILabelRepository
    {
        Task AddLabelAsync(Labels label);
        Task DeleteLabelAsync(Guid labelId);
        Task<Labels?> GetLabelByIdAsync(Guid labelId);
        Task UpdateLabelAsync(Labels label);


    }
}

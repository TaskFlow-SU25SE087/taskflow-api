using System.Reflection.Emit;
using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ILabelService
    {
        Task AddLabel(AddLabelRequest request);
        Task DeleteLabel(Guid labelId);
        Task UpdateLabel(UpdateLabelRequest request);

    }
}

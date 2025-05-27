using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ITaskFlowAuthorizationService
    {
        Task<bool> AuthorizeAsync(Guid projectId, params ProjectRole[] allowedRoles);
    }
}

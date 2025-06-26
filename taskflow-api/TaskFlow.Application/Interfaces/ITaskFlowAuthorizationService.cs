using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ITaskFlowAuthorizationService
    {
        Task<bool> AuthorizeAsync(Guid projectId, params ProjectRole[] allowedRoles);
        Task<Guid> AuthorizeAndGetMemberAsync(Guid projectId, params ProjectRole[] allowedRoles);
    }
}

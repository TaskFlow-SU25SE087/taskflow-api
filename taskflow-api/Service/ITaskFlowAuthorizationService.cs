using taskflow_api.Enums;

namespace taskflow_api.Service
{
    public interface ITaskFlowAuthorizationService
    {
        Task<bool> AuthorizeAsync(Guid projectId, params ProjectRole[] allowedRoles);
    }
}

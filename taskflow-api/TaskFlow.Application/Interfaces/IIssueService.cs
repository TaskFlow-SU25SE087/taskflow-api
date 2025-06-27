using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IIssueService
    {
        Task CreateTaskIssue(Guid memberId, Guid projectId, Guid TaskId, CreateTaskIssueRequest request);
    }
}

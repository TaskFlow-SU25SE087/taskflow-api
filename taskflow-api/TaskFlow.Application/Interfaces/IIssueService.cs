using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IIssueService
    {
        Task CreateTaskIssue(Guid memberId, Guid projectId, Guid TaskId, CreateTaskIssueRequest request);
        Task<List<IssueDetailResponse>> GetAllIssue(Guid ProjectId);
    }
}

using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IIssueService
    {
        Task CreateTaskIssue(Guid memberId, Guid projectId, Guid TaskId, CreateTaskIssueRequest request);
        Task<List<IssueDetailResponse>> GetAllIssue(Guid ProjectId);
        Task<List<IssueDetailResponse>> FilterIssue(Guid ProjectId, IssueStatus? status, TypeIssue? type, TaskPriority? priority);
    }
}

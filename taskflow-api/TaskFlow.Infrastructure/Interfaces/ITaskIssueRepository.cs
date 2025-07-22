using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ITaskIssueRepository
    {
        Task CreateTaskIssueAsync(Issue data);
        Task<List<IssueDetailResponse>> GetIssue(Guid projectId, IssueStatus? status = null, 
            TypeIssue? type = null, TaskPriority? priority = null);
    }
}

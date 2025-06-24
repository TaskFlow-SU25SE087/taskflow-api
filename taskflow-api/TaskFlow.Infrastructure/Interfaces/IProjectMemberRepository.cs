using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IProjectMemberRepository
    {
        Task CreateProjectMemeberAsync(ProjectMember data);
        Task<ProjectMember?> FindMemberInProject(Guid projectId, Guid userId);
        Task UpdateMember(ProjectMember data);
        Task<int> GetProjectCountByUserIdAsync(Guid userId);
        Task<int> GetActiveMembersCount(Guid ProjectId);
        Task<bool> IsUserInProjectAsync(Guid projectId, Guid userId);

    }
}

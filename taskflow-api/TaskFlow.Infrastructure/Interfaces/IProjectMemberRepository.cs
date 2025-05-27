using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IProjectMemberRepository
    {
        Task CreateProjectMemeberAsync(ProjectMember data);
    }
}

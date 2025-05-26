using taskflow_api.Entity;

namespace taskflow_api.Repository
{
    public interface IProjectMemberRepository
    {
        Task CreateProjectMemeberAsync(ProjectMember data);
    }
}

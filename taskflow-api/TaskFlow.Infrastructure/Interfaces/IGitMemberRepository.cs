using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IGitMemberRepository
    {
        Task CreateGitMember(GitMember data);
        Task CreateListGitMember(List<GitMember> data);
    }
}

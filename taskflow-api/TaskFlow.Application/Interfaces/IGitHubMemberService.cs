using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IGitHubMemberService
    {
        Task CreateGitMember(Guid ProjectPartId, Guid ProjectMemberId, CreateGitMemberRequest gitMember);
        Task AddGitLocal(Guid Id, CreateGitMemberRequest gitMember);
    }
}

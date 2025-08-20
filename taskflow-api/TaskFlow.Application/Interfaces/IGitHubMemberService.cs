using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IGitHubMemberService
    {
        Task CreateGitMember(Guid ProjectPartId, Guid ProjectMemberId, CreateGitMemberRequest gitMember);
        Task AddGitLocal(Guid Id, CreateGitMemberRequest gitMember);
        Task<List<GitMemberResponse>> GitMember(Guid projectPartId);
    }
}

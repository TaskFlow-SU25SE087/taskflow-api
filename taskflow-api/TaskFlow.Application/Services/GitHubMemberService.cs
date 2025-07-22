using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class GitHubMemberService : IGitHubMemberService
    {
        private readonly IGitMemberRepository _gitMemberRepository;

        public GitHubMemberService(IGitMemberRepository gitMemberRepository)
        {
            _gitMemberRepository = gitMemberRepository;
        }

        public async Task AddGitLocal(Guid Id, CreateGitMemberRequest gitMember)
        {
            var data = await _gitMemberRepository.GetGitMemberById(Id);
            if (data == null)
            {
                throw new AppException(ErrorCode.GitMemberNotFound);
            }
            data.NameLocal = gitMember.NameLocal;
            data.EmailLocal = gitMember.EmailLocal;
            await _gitMemberRepository.Update(data);
        }

        public async Task CreateGitMember(Guid ProjectPartId, Guid ProjectMemberId, CreateGitMemberRequest gitMember)
        {
            var data = new GitMember
            {
                ProjectMemberId = ProjectMemberId,
                ProjectPartId = ProjectPartId,
                NameLocal = gitMember.NameLocal,
                EmailLocal = gitMember.EmailLocal
            };
             await _gitMemberRepository.CreateGitMember(data);
        }
    }
}

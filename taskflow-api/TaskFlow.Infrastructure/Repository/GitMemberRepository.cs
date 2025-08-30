using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class GitMemberRepository : IGitMemberRepository
    {
        private readonly TaskFlowDbContext _context;

        public GitMemberRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task CreateGitMember(GitMember data)
        {
            _context.GitMembers.Add(data);
            await _context.SaveChangesAsync();
        }

        public async Task CreateListGitMember(List<GitMember> data)
        {
            _context.GitMembers.AddRange(data);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllProjectPart(Guid projectPartId)
        {
            var gitMembers = _context.GitMembers
                .Where(gm => gm.ProjectPartId == projectPartId);
            _context.GitMembers.RemoveRange(gitMembers);
            await _context.SaveChangesAsync();
        }

        public async Task<GitMember?> GetGitMemberById(Guid id)
        {
            var gitMember = await _context.GitMembers.FindAsync(id);
            return gitMember;
        }

        public Task<List<GitMemberResponse>> GetListGitMemberByIProjectPartId(Guid projectPartId)
        {
            return _context.GitMembers
                .Where(gm => gm.ProjectPartId == projectPartId)
                .Select(gm => new GitMemberResponse
                {
                    Id = gm.Id,
                    ProjectMemberId = gm.ProjectMemberId,
                    GitName = gm.GitName,
                    GitEmail = gm.GitEmail,
                    GitAvatarUrl = gm.GitAvatarUrl,
                    NameLocal = gm.NameLocal,
                    EmailLocal = gm.EmailLocal,
                })
                .ToListAsync();
        }

        public async Task<GitMember?> GetMemberByNameAndEmailLocal(string name, string email, Guid projectPartId)
        {
            return await _context.GitMembers
                .FirstOrDefaultAsync(gm => gm.NameLocal == name && gm.EmailLocal == email && gm.ProjectPartId == projectPartId);
        }

        public async Task Update(GitMember data)
        {
            _context.GitMembers.Update(data);
            await _context.SaveChangesAsync();
        }
    }
}

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
    }
}

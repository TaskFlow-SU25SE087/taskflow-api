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
    }
}

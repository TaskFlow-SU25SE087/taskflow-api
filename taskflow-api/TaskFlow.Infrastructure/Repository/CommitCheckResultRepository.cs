using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class CommitCheckResultRepository : ICommitCheckResultRepository
    {
        private readonly TaskFlowDbContext _context;

        public CommitCheckResultRepository(TaskFlowDbContext context)
        {
            _context = context;
        }
        public async Task CreateCommitResult(CommitCheckResult data)
        {
            await _context.CommitCheckResults.AddAsync(data);
            await _context.SaveChangesAsync();
        }
    }
}

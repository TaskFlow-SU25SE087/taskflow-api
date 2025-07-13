using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class CommitRecordRepository : ICommitRecordRepository
    {
        private readonly TaskFlowDbContext _context;

        public CommitRecordRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task Create(CommitRecord data)
        {
            await _context.CommitRecords.AddAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByCommitId(string commitId)
        {
            return await _context.CommitRecords.AnyAsync(c => c.CommitId == commitId);
        }

        public async Task<CommitRecord?> GetById(Guid commitId)
        {
            var commitRecord = await _context.CommitRecords
                .Include(c => c.ProjectPart)
                .FirstOrDefaultAsync(c => c.Id == commitId);
            return commitRecord;
        }

        public async Task Update(CommitRecord data)
        {
            _context.CommitRecords.Update(data);
            await _context.SaveChangesAsync();
        }
    }
}

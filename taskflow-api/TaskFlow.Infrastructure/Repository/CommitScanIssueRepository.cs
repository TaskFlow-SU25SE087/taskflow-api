using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class CommitScanIssueRepository : ICommitScanIssueRepository
    {
        private readonly TaskFlowDbContext _context;

        public CommitScanIssueRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(CommitScanIssue issue)
        {
            await _context.CommitScanIssues.AddAsync(issue);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CommitScanIssue>> GetByCommitCheckResultIdAsync(Guid commitRecordId)
        {
            return await _context.CommitScanIssues
            .Where(x => x.CommitRecordId == commitRecordId)
            .ToListAsync();
        }
    }
}

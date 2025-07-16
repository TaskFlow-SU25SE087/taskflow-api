using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Response;
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

        public Task<int> CountCommitByProjectPart(Guid projectPart)
        {
            return _context.CommitRecords
                .CountAsync(c => c.ProjectPartId == projectPart);
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

        public Task<List<CommitRecordResponse>> GetCommitRecordsByPartId(Guid projectPartId, int page, int pageSize)
        {
            return _context.CommitRecords
                .Where(c => c.ProjectPartId == projectPartId)
                .OrderByDescending(c => c.PushedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommitRecordResponse
                {
                    CommitId = c.CommitId,
                    Pusher = c.Pusher,
                    PushedAt = c.PushedAt,
                    Status = c.Status,
                    CommitUrl = c.CommitUrl,
                    CommitMessage = c.CommitMessage,
                    ResultSummary = c.ResultSummary,
                    ExpectedFinishAt = c.ExpectedFinishAt,
                    QualityGateStatus = c.QualityGateStatus,
                    Bugs = c.Bugs,
                    Vulnerabilities = c.Vulnerabilities,
                    CodeSmells = c.CodeSmells,
                    SecurityHotspots = c.SecurityHotspots,
                    DuplicatedLines = c.DuplicatedLines,
                    DuplicatedBlocks = c.DuplicatedBlocks,
                    DuplicatedLinesDensity = c.DuplicatedLinesDensity,
                    Coverage = c.Coverage,
                    ScanDuration = c.ScanDuration,
                    Result = c.Result,
                })
                .ToListAsync();
        }

        public async Task Update(CommitRecord data)
        {
            _context.CommitRecords.Update(data);
            await _context.SaveChangesAsync();
        }
    }
}

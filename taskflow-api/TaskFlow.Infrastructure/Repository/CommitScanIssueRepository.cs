using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Response;
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

        public async Task<bool> CheckIssueReult(string message, string lineContent, string blamedEmail, string blamedName, string cleanFilePath)
        {
            return await _context.CommitScanIssues
                .AnyAsync(x => x.Message == message &&
                               x.LineContent == lineContent &&
                               x.BlamedGitEmail == blamedEmail &&
                               x.BlamedGitName == blamedName &&
                               x.FilePath == cleanFilePath);
        }

        public async Task CreateAsync(CommitScanIssue issue)
        {
            await _context.CommitScanIssues.AddAsync(issue);
            await _context.SaveChangesAsync();
        }

        public Task<List<CommitDetailResponse>> GetByCommitCheckResultId(string commitId)
        {
            return _context.CommitScanIssues
                .Where(x => x.CommitRecord.CommitId == commitId)
                .Select(x => new CommitDetailResponse
                {
                    Rule = x.Rule,
                    Severity = x.Severity,
                    Message = x.Message,
                    FilePath = x.FilePath,
                    Line = x.Line,
                    LineContent = x.LineContent ?? string.Empty,
                    BlamedGitEmail = x.BlamedGitEmail ?? string.Empty,
                    BlamedGitName = x.BlamedGitName ?? string.Empty
                })
                .ToListAsync();
        }

        public async Task<List<CommitScanIssue>> GetByCommitCheckResultIdAsync(Guid commitRecordId)
        {
            return await _context.CommitScanIssues
            .Where(x => x.CommitRecordId == commitRecordId)
            .ToListAsync();
        }
    }
}

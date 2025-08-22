using taskflow_api.TaskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Helpers;
using System;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class TermRepository : ITermRepository
    {
        private readonly TaskFlowDbContext _context;
        private readonly AppTimeProvider _timeProvider;

        public TermRepository(TaskFlowDbContext context, AppTimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task CreateTermAsync(Term data)
        {
            await _context.Terms.AddAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTermAsync(Guid termId)
        {
            await _context.Terms
                .Where(t => t.Id == termId)
                .ExecuteDeleteAsync();
        }

        public Task<List<Term>> GetAllTermsAsync(int page, int pageSize)
        {
            return _context.Terms
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<List<Term>> GetAllActiveTermsAsync()
        {
            return _context.Terms
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
        }

        public async Task<DateTime?> GetLatestTermEndDateAsync()
        {
            return await _context.Terms
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.EndDate)
                .Select(t => t.EndDate)
                .FirstOrDefaultAsync();
        }

        public Task<Term?> GetTermByIdAsync(Guid termId)
        {
            return _context.Terms
                .FirstOrDefaultAsync(t => t.Id == termId);
        }

        public async Task<Guid> GetTermIdAsync(string season, int year)
        {
            return await _context.Terms
                .Where(t => t.Season == season && t.Year == year)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateTermAsync(Term data)
        {
            _context.Terms.Update(data);
            await _context.SaveChangesAsync();
        }

        public async Task<Term?> GetCurrentTermAsync()
        {
            return await _context.Terms
                .Where(t => t.IsActive && t.StartDate <= _timeProvider.Now && _timeProvider.Now <= t.EndDate)
                .OrderByDescending(t => t.StartDate)
                .FirstOrDefaultAsync();
        }
    }
}

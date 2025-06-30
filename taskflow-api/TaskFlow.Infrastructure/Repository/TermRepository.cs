using taskflow_api.TaskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class TermRepository : ITermRepository
    {
        private readonly TaskFlowDbContext _context;

        public TermRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task CreateTermAsync(Term data)
        {
            await _context.Terms.AddAsync(data);
            await _context.SaveChangesAsync();
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

        public async Task<DateTime> GetLatestTermEndDateAsync()
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
    }
}

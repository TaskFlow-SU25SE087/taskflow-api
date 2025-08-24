using taskflow_api.TaskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Helpers;
using System;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Application.DTOs.Response;

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

        public Task<List<TermResponse>> GetAllTermsAsync(int page, int pageSize)
        {
            return _context.Terms
                .Include(t => t.Users.Where(u => u.Role != UserRole.Admin))
                .OrderByDescending(t => t.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TermResponse
                {
                    Id = t.Id,
                    Season = t.Season,
                    Year = t.Year,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    IsActive = t.IsActive,
                    Users = t.Users.Select(u => new UserResponseInTerm
                    {
                        Id = u.Id,
                        Avatar = u.Avatar,
                        FullName = u.FullName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Role = u.Role.ToString(),
                        StudentId = u.StudentId,
                        TermSeason = t.Season,
                        TermYear = t.Year
                    }).ToList()
                })
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

        public async Task<List<User>> GetPastUser(Guid termId)
        {
            var termIdStr = termId.ToString();
            return await _context.Users
                 .Where(u => u.PastTerms != null &&
                        (u.PastTerms.Contains(termIdStr + ",")
                         || u.PastTerms.Contains("," + termIdStr)
                         || u.PastTerms == termIdStr))
                .ToListAsync();
        }

        public async Task<int> CountTerm()
        {
            return await _context.Terms.CountAsync();
        }
    }
}

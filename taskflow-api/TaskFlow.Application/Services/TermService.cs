using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Repository;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class TermService : ITermService
    {
        private readonly ITermRepository _termRepository;
        private readonly UserManager<User> _userManager;

        public TermService(ITermRepository termRepository, UserManager<User> userManager)
        {
            _termRepository = termRepository;
            _userManager = userManager;
        }

        public async Task CreateTerm(CreateTerm request)
        {
            //check dates
            var LatestTermEndDate = await _termRepository.GetLatestTermEndDateAsync();
            if (request.StartDate >= request.EndDate || (LatestTermEndDate.HasValue && request.StartDate < LatestTermEndDate.Value))
            {
                throw new AppException(ErrorCode.InvalidTermDates);
            }
            var term = new Term
            {
                Season = request.Season,
                Year = request.Year,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true
            };
            await _termRepository.CreateTermAsync(term);
        }

        public async Task LockTerm(Guid id)
        {
            var term = await _termRepository.GetTermByIdAsync(id);
            if (term == null)
            {
                throw new AppException(ErrorCode.TermNotFound);
            }
            term.IsActive = !term.IsActive;
            await _termRepository.UpdateTermAsync(term);
        }

        public async Task<PagedResult<TermResponse>> GetListTerm(int page)
        {
            const int PageSize = 10; // Define your page size
            int TotalItems = await _termRepository.CountTerm();
            int TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            // Define your page size
            var terms = await _termRepository.GetAllTermsAsync(page, PageSize);
            //if (terms == null || !terms.Any())
            //{
            //    throw new AppException(ErrorCode.TermNotFound);
            //}
            //get list term
            var termIds = terms.Select(t => t.Id).ToList();
            var termIdStrings = termIds.Select(id => id.ToString()).ToList();
            //get all past users
            var allPastUsers = await _userManager.Users
                .Where(u => u.PastTerms != null && u.Role != UserRole.Admin &&
                            termIdStrings.Any(tid =>
                                u.PastTerms == tid ||
                                u.PastTerms.StartsWith(tid + ",") ||
                                u.PastTerms.EndsWith("," + tid) ||
                                u.PastTerms.Contains("," + tid + ",")))
                .ToListAsync();

            // add PastUsers 
            foreach (var term in terms)
            {
                var key = term.Id.ToString();
                var pastForThisTerm = allPastUsers
                    .Where(u => u.TermId == null || u.TermId != term.Id)
                    .Where(u => u.PastTerms != null && PastTermsContains(u.PastTerms, key))
                    .ToList();

                // Set + duplicate type by id od user 
                var current = term.Users?
                    .Where(u => u.Role != UserRole.Admin.ToString())
                    .ToList() ?? new List<UserResponseInTerm>();

                // Combine current and past users for this term
                term.Users = current
                    .Concat(pastForThisTerm.Select(u => new UserResponseInTerm
                    {
                        Id = u.Id,
                        Avatar = u.Avatar,
                        FullName = u.FullName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Role = u.Role.ToString(),
                        StudentId = u.StudentId,
                        TermSeason = term.Season,
                        TermYear = term.Year
                    }))
                    .DistinctBy(u => u.Id)
                    .ToList();
            }
            // Return paged result
            var pagedResult = new PagedResult<TermResponse>
            {
                Items = terms,
                TotalItems = TotalItems,
                PageNumber = page,
                PageSize = PageSize,
                TotalPages = TotalPages
            };
            return pagedResult;
        }

        public async Task<List<Term>> GetAllTermsForAdmin()
        {
            var terms = await _termRepository.GetAllActiveTermsAsync();
            if (terms == null || !terms.Any())
            {
                throw new AppException(ErrorCode.TermNotFound);
            }
            return terms;
        }

        public Task<Term?> GetTermById(Guid id)
        {
            var term = _termRepository.GetTermByIdAsync(id);
            if (term == null)
            {
                throw new AppException(ErrorCode.TermNotFound);
            }
            return term;
        }

        public async Task UpdateTerm(Guid termId, UpdateTerm request)
        {
            var term = await _termRepository.GetTermByIdAsync(termId);
            if (term == null)
            {
                throw new AppException(ErrorCode.TermNotFound);
            }
            //check dates
            if (request.StartDate >= request.EndDate || request.StartDate < term.StartDate)
            {
                throw new AppException(ErrorCode.InvalidTermDates);
            }
            term.Season = request.Season;
            term.Year = request.Year;
            term.StartDate = request.StartDate;
            term.EndDate = request.EndDate;
            await _termRepository.UpdateTermAsync(term);
        }

        public async Task Delete(Guid id)
        {
            var term = _termRepository.GetTermByIdAsync(id);

            //check project or user in term
            if (term == null || term.Result == null)
            {
                throw new AppException(ErrorCode.TermNotFound);
            }

            if (term.Result.Projects.Any() || term.Result.Users.Any())
            {
                throw new AppException(ErrorCode.TermHasProjectsOrUsers);
            }
            await _termRepository.DeleteTermAsync(id);
        }
        private bool PastTermsContains(string pastTerms, string key)
        {
            return pastTerms == key
        || pastTerms.StartsWith(key + ",")
        || pastTerms.EndsWith("," + key)
        || pastTerms.Contains("," + key + ",");
        }
    }
}

using taskflow_api.TaskFlow.Application.DTOs.Request;
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

        public TermService(ITermRepository termRepository)
        {
            _termRepository = termRepository;
        }

        public async Task CreateTerm(CreateTerm request)
        {
            //check dates
            var LatestTermEndDate = await _termRepository.GetLatestTermEndDateAsync();
            if (request.StartDate >= request.EndDate || request.StartDate < LatestTermEndDate)
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

        public async Task DeleteTerm(Guid id)
        {
            var term = await _termRepository.GetTermByIdAsync(id);
            if (term == null)
            {
                throw new AppException(ErrorCode.TermNotFound);
            }
            term.IsActive = false;
            await _termRepository.UpdateTermAsync(term);
        }

        public async Task<List<Term>> GetListTerm(int page)
        {
            page = Math.Max(page, 1);
            var PageSize = 10;
            // Define your page size
            var terms = await _termRepository.GetAllTermsAsync(page, PageSize);
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
    }
}

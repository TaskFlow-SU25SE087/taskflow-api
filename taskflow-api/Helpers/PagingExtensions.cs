using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using taskflow_api.Model.Common;

namespace taskflow_api.Helpers
{
    public static class PagingExtensions
    {
        public static async Task<PagedResult<T>> ToPagedListAsync<T>(
            this IQueryable<T> query, PagingParams pagingParams)
        {
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip(pagingParams.Skip)
                .Take(pagingParams.PageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pagingParams.PageSize);

            return new PagedResult<T>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                PageNumber = pagingParams.PageNumber,
                PageSize = pagingParams.PageSize
            };
        }
    }

}

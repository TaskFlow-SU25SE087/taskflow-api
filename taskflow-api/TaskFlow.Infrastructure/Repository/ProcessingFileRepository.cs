using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Infrastructure.Repository
{
    public class ProcessingFileRepository : IProcessingFileRepository
    {
        private readonly TaskFlowDbContext _context;

        public ProcessingFileRepository(TaskFlowDbContext context)
        {
            _context = context;
        }

        public async Task CreateProcessingFileAsync(ProcessingFile processingFile)
        {
             _context.ProcessingFiles.Add(processingFile);
             await _context.SaveChangesAsync();
        }

        public Task<int> GetCountAllProcessFile()
        {
            return _context.ProcessingFiles.CountAsync();
        }

        public async Task<ProcessingFile> GetProcessingFileByIdAsync(Guid id)
        {
            var processingFile = await _context.ProcessingFiles
                .FirstOrDefaultAsync(pf => pf.Id == id);
            if (processingFile == null)
            {
                throw new KeyNotFoundException($"Processing file with ID {id} not found.");
            }
            return processingFile;
        }

        public async Task<List<ProcessingFile>> GetProcessingFiles(PagingParams pagingParams)
        {
            return await _context.ProcessingFiles
                .OrderByDescending(pf => pf.CreatedAt)
                .Skip((pagingParams.PageNumber - 1) * pagingParams.PageSize)
                .Take(pagingParams.PageSize)
                .ToListAsync();
        }

        public async Task UpdateProcessingFileAsync(ProcessingFile processingFile)
        {
            _context.ProcessingFiles.Update(processingFile);
            var result = await _context.SaveChangesAsync();
        }
    }
}

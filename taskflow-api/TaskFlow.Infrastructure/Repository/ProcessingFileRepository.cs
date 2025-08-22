using Microsoft.EntityFrameworkCore;
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

        public async Task UpdateProcessingFileAsync(ProcessingFile processingFile)
        {
            _context.ProcessingFiles.Update(processingFile);
            var result = await _context.SaveChangesAsync();
        }
    }
}

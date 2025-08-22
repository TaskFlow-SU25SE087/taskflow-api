
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IProcessingFileRepository
    {
        Task CreateProcessingFileAsync(ProcessingFile processingFile);
        Task<ProcessingFile> GetProcessingFileByIdAsync(Guid id);
        Task UpdateProcessingFileAsync(ProcessingFile processingFile);
    }
}

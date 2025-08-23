
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface IProcessingFileRepository
    {
        Task CreateProcessingFileAsync(ProcessingFile processingFile);
        Task<int> GetCountAllProcessFile();
        Task<ProcessingFile> GetProcessingFileByIdAsync(Guid id);
        Task<List<ProcessingFile>> GetProcessingFiles(PagingParams pagingParams);
        Task UpdateProcessingFileAsync(ProcessingFile processingFile);
    }
}

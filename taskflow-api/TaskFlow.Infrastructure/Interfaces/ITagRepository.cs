using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ITagRepository
    {
        Task AddTagAsync(Tag Tag);
        Task DeleteTagAsync(Guid TagId);
        Task<Tag?> GetTagByIdAsync(Guid TagId);
        Task UpdateTagAsync(Tag Tag);


    }
}

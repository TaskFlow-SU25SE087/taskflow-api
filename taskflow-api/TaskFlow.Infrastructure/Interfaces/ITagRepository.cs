using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Infrastructure.Interfaces
{
    public interface ITagRepository
    {
        Task AddTagAsync(Tag Tag);
        Task DeleteTagAsync(Guid TagId);
        Task<Tag?> GetTagByIdAsync(Guid TagId);
        Task UpdateTagAsync(Tag Tag);
        Task<List<Tag>> GetListTagsync(Guid ProjectId);
        Task<bool> CheckNameTagAsync(Guid ProjectId, string Name);

    }
}

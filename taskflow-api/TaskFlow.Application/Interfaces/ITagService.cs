using System.Reflection.Emit;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ITagService
    {
        Task AddTag(Guid ProjectID, AddTagRequest request);
        Task DeleteTag(Guid TagId);
        Task UpdateTag(Guid ProjectId, Guid TagId, UpdateTagRequest request);
        Task<List<TagResporn>> GetListTag(Guid ProjectId);

    }
}

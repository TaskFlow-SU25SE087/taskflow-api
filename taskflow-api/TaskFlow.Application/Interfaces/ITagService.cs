using System.Reflection.Emit;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ITagService
    {
        Task AddTag(AddTagRequest request);
        Task DeleteTag(Guid TagId);
        Task UpdateTag(UpdateTagRequest request);
        Task<List<TagResporn>> GetListTag(Guid ProjectId);

    }
}

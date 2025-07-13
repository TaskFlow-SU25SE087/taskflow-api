using Newtonsoft.Json.Linq;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IProjectPartService
    {
        Task CreatePart(Guid ProjectId, CreateProjectPartRequest request);
        Task ConnectRepo(Guid partId, ConnectRepoRequest request);
        Task ProcessGitHubPushEvent(JObject data);
        Task <List<ProjectPartResponse>> GetAllRepositories(Guid ProjectId);
    }
}

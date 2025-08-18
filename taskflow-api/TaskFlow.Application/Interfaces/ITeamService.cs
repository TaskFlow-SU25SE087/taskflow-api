using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ITeamService
    {
        Task<List<TeamResponse>> GetTeamsByTermAsync(Guid termId);
        Task<TeamResponse?> GetTeamByIdAsync(Guid projectId);
        Task<List<TeamResponse>> GetAllTeamsAsync();
    }
}

using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface ITeamActivityReportService
    {
        Task<TeamActivityReportResponse> GenerateTeamActivityReportAsync(Guid projectId, TeamActivityReportRequest request);
        Task<MemberActivityResponse> GenerateMemberActivityReportAsync(Guid projectId, Guid memberId, TeamActivityReportRequest request);
        Task<BurndownChartResponse> GetBurndownChartAsync(Guid projectId, Guid sprintId);
    }
}

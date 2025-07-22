using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IMemberService
    {
        public Task<bool> AddMember(Guid ProjectId, AddMemberRequest request);
        public Task<bool> VerifyJoinProject(string token);
        public Task<bool> RemoveMember(Guid projectId, Guid projectMemberId);
        public Task<bool> LeaveTheProject(Guid projectId);
        public Task<List<MemberResponse>> GetAllMemberInProject(Guid projectId);
        Task<ProjectMemberResponse> GetMeInProject(Guid ProjectId, Guid ProjectMemberId);
        Task AddSystemUSer(Guid ProjectId);
    }
}

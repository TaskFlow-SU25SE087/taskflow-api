using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IMemberService
    {
        public Task<bool> AddMember(AddMemberRequest request);
        public Task<bool> VerifyJoinProject(string token);
        public Task<bool> RemoveMember(Guid projectId, Guid userId);
        public Task<bool> LeaveTheProject(Guid projectId);
    }
}

using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IProjectService
    {
        public Task<ProjectResponse> CreateProject(CreateProjectRequest request);
        public Task<ProjectResponse> UpdateProject(UpdateProjectRequest request);

        //public Task<bool> AddMember(AddMemberRequest request);
        //public Task<bool> VerifyJoinProject(string token);
        //public Task<bool> RemoveMember(Guid projectId, Guid userId);
        //public Task<bool> LeaveTheProject(Guid projectId);
        //public Task<bool> CreateBoard(CreateBoardRequest request);
        //public Task<bool> UpdateBoard(UpdateBoardRequest request);
        //public Task<bool> DeleteBoard(Guid boardId);
        //public Task<bool> UpdateBoardOrder(List<UpdateBoardRequest> request);
        //public Task<bool> CreateSprint(CreateSprintRequest request);
        //public Task<bool> UpdateSprint(UpdateSprintRequest request);
    }
}

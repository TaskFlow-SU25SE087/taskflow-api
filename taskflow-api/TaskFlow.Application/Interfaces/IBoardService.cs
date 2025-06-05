using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IBoardService
    {
        public Task<bool> CreateBoard(CreateBoardRequest request);
        public Task<bool> UpdateBoard(UpdateBoardRequest request);
        public Task<bool> DeleteBoard(Guid boardId);
        public Task<bool> UpdateBoardOrder(List<UpdateBoardRequest> request);
    }
}

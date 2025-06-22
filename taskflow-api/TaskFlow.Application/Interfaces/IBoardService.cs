using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IBoardService
    {
        public Task<bool> CreateBoard(Guid ProjectID, CreateBoardRequest request);
        public Task<bool> UpdateBoard(Guid ProjectId, Guid BoardId , UpdateBoardRequest request);
        public Task<bool> DeleteBoard(Guid boardId);
        public Task<bool> UpdateBoardOrder(List<UpdateOrderBoardRequest> request);
        public Task<List<BoardResponse>> ListBoardAsync(Guid ProjectId);
    }
}

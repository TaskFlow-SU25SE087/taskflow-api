using AutoMapper;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Repository;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IMapper _mapper;
        public BoardService(IBoardRepository boardRepository, IMapper mapper)
        {
            _boardRepository = boardRepository;
            _mapper = mapper;
        }

        public async Task<bool> CreateBoard(Guid ProjectID, CreateBoardRequest request)
        {
            if (request == null)
            {
                throw new AppException(ErrorCode.CannotCreateBoard);
            }
            int Order = await _boardRepository.GetMaxOrder(ProjectID) + 1;
            var board = new Board
            {
                Id = Guid.NewGuid(),
                ProjectId = ProjectID,
                Name = request.Name,
                Description = request.Description,
                Order = Order,
                IsActive = true
            };
            await _boardRepository.CreateBoardsAsync(board);
            return true;
        }

        public async Task<bool> DeleteBoard(Guid boardId)
        {
            //No condition set to not be deleted. Will be in the future.
            var boardDelete = await _boardRepository.GetBoardByIdAsync(boardId);
            if (boardDelete == null)
            {
                throw new AppException(ErrorCode.BoardNotFound);
            }
            //check max board order
            var maxOrder = await _boardRepository.GetMaxOrder(boardDelete.ProjectId);
            if (boardDelete.Order <= 3 )
            {
                throw new AppException(ErrorCode.CannotDeleteBoard);
            }
            //Set the board as inactive
            boardDelete!.IsActive = false;
            await _boardRepository.UpdateBoard(boardDelete);

            //Update the order of the boards after the deleted board
            var listBoradsUpdate = await _boardRepository.GetBoardsAfterOrderAsync(boardDelete.Order);
            if (listBoradsUpdate != null)
            {
                foreach (var board in listBoradsUpdate)
                {
                    board.Order--;
                }
                await _boardRepository.UpdateListBoardsAsync(listBoradsUpdate);
            }
            return true;
        }

        public async Task<List<BoardResponse>> ListBoardAsync(Guid ProjectId)
        {
            var boards = await _boardRepository.GetListBoardAsync(ProjectId);
            //var result = _mapper.Map<List<BoardResponse>>(boards);
            return boards;
        }

        public async Task<bool> UpdateBoard(Guid ProjectId, Guid BoardId, UpdateBoardRequest request)
        {
            var board = await _boardRepository.GetBoardByIdAsync(BoardId);
            if (board == null)
            {
                throw new AppException(ErrorCode.BoardNotFound);
            }
            //Update the board
            board!.Name = request.Name;
            board.Description = request.Description;

            await _boardRepository.UpdateBoard(board);
            return true;
        }

        public async Task<bool> UpdateBoardOrder(List<UpdateOrderBoardRequest> request)
        {
            if (request == null || request.Count == 0)
            {
                throw new AppException(ErrorCode.CannotUpdateBoard);
            }
            //get all board id from request
            var boardIds = request.Select(x => x.Id).ToList();
            //get all boards by ids from database
            var boards = await _boardRepository.GetBoardsByIdsAsync(boardIds);
            if (boards.Count != boardIds.Count)
            {
                throw new AppException(ErrorCode.BoardNotFound);
            }
            //Update the order of the boards
            foreach (var board in boards)
            {
                var newOrder = request.First(x => x.Id == board.Id).Order;
                board.Order = newOrder;
            }
            await _boardRepository.UpdateListBoardsAsync(boards);
            return true;

            //var listBoards = _mapper.Map<List<Board>>(request);
            //if (listBoards == null || listBoards.Count == 0)
            //{
            //    throw new AppException(ErrorCode.CannotUpdateBoard);
            //}
            ////Update the order of the boards
            //await _boardRepository.UpdateListBoardsAsync(listBoards);
            //return true;
        }

    }
}

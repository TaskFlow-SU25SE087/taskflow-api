using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class SprintService : ISprintService
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly ITaskProjectRepository _taskProjectRepository;

        public SprintService(ISprintRepository repository, ITaskProjectRepository taskProjectRepository)
        {
            _sprintRepository = repository;
            _taskProjectRepository = taskProjectRepository;
        }

        public async Task AddTasksToSprint(Guid ProjectId, Guid SprintId, List<Guid> TaskID)
        {
            List<TaskProject> tasks = await _taskProjectRepository.GetListTasksByIdsAsync(TaskID);
            foreach (var task in tasks)
            {
                task.SprintId = SprintId;
            }
            await _taskProjectRepository.UpdateListTaskAsync(tasks);
        }

        public async Task ChangeStatusSprint(Guid SpringId, SprintStatus status)
        {
            var sprint = await _sprintRepository.GetSprintByIdAsync(SpringId);
            sprint!.Status = status;
            await _sprintRepository.UpdateSprintAsync(sprint);

            if (status.Equals(SprintStatus.Completed))
            {
                var lisktaskproject = await _taskProjectRepository.GetListTasksBySprintsIdsAsync(SpringId);
                foreach (var task in lisktaskproject)
                {
                    task.Sprint = null;
                    task.Note = task.Note + " " + DateTime.UtcNow + " End sprint: " + sprint.Name; 
                }
                await _taskProjectRepository.UpdateListTaskAsync(lisktaskproject);
            }
        }

        public async Task<bool> CreateSprint(Guid ProjectId, CreateSprintRequest request)
        {
            var newSprint = new Sprint
            {
                ProjectId = ProjectId,
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true,
                Status = SprintStatus.NotStarted
            };
            await _sprintRepository.CreateSprintAsync(newSprint);
            return true;
        }

        public async Task<List<SprintResponse>> ListPrints(Guid ProjectId)
        {
            var result = await _sprintRepository.GetListPrintAsync(ProjectId);
            return result;
        }

        public async Task<bool> UpdateSprint(Guid ProjectId, Guid SprintId, UpdateSprintRequest request)
        {
            var sprint = await _sprintRepository.GetSprintByIdAsync(SprintId);
            if (sprint == null || sprint.ProjectId != ProjectId)
            {
                // Sprint not found or Project mismatch
                throw new AppException(ErrorCode.CannotUpdateSprint);
            }

            //update sprint
            sprint.Name = request.Name;
            sprint.Description = request.Description;
            sprint.StartDate = request.StartDate;
            sprint.EndDate = request.EndDate;
            sprint.Status = request.Status;
            await _sprintRepository.UpdateSprintAsync(sprint);
            return true;
        }

    }
}

using System;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class SprintService : ISprintService
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly ITaskProjectRepository _taskProjectRepository;
        private readonly AppTimeProvider _timeProvider;

        public SprintService(ISprintRepository repository, ITaskProjectRepository taskProjectRepository,
            AppTimeProvider timeProvider)
        {
            _sprintRepository = repository;
            _taskProjectRepository = taskProjectRepository;
            _timeProvider = timeProvider;
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
            if (status.Equals(SprintStatus.InProgress)) //start sprint
            {
                bool checkSprintStart = await _sprintRepository.CheckSprintStartDate(sprint.ProjectId);
                if (checkSprintStart)
                {
                    throw new AppException(ErrorCode.SprintAlreadyInProgress);
                }
                if (sprint.StartDate <= DateTime.UtcNow)
                {
                    throw new AppException(ErrorCode.CannotStartSprint);
                }
                await _sprintRepository.UpdateSprintAsync(sprint);
            }
            else if (status.Equals(SprintStatus.Completed))// complete sprint
            {
                // new next sprint
                var lastSprint = await _sprintRepository.GetLastSprint(sprint.ProjectId);
                var newSprint = new Sprint
                {
                    Id = Guid.NewGuid(),
                    ProjectId = sprint.ProjectId,
                    Name = "Sprint " + (lastSprint == null ? 1 : lastSprint.Name.Split(' ').LastOrDefault() + 1),
                    Description = "Next sprint after " + sprint.Name,
                    StartDate = lastSprint!.EndDate,
                    EndDate = _timeProvider.Now.AddDays(14), // Example: 2 weeks duration
                    IsActive = true,
                    Status = SprintStatus.NotStarted,
                };
                var lisktaskproject = await _taskProjectRepository.GetListTasksBySprintsIdsAsync(SpringId);
                foreach (var task in lisktaskproject)
                {
                    task.SprintId = newSprint.Id;
                    task.Note = task.Note + " " + DateTime.UtcNow + " End sprint: " + sprint.Name; 
                }
                await _taskProjectRepository.UpdateListTaskAsync(lisktaskproject);
                await _sprintRepository.UpdateSprintAsync(sprint);
            }
        }

        public async Task<bool> CreateSprint(Guid ProjectId, CreateSprintRequest request)
        {
            var existingSprint = await _sprintRepository.CheckSprintName(ProjectId, request.Name);
            if (existingSprint)
            {
                throw new AppException(ErrorCode.SprintNameAlreadyExists);
            }
            if (request.StartDate < DateTime.UtcNow)
            {
                throw new AppException(ErrorCode.CannotCreateSprint);
            }

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

        public Task<SprintResponse?> GetCurrentSprint(Guid ProjectId)
        {
            return _sprintRepository.GetCurrentSprint(ProjectId);
        }

        public async Task<List<TaskProjectResponse>> GetTaskInSprints(Guid ProjectId, Guid SprintId)
        {
            return await _taskProjectRepository.GetListTaskBySprintIdAsync(SprintId); 
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

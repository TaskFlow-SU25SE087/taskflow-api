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
        private readonly ISprintMeetingLogsService _sprintMeetingLogs;

        public SprintService(ISprintRepository repository, ITaskProjectRepository taskProjectRepository,
            AppTimeProvider timeProvider, ISprintMeetingLogsService sprintMeetingLogs)
        {
            _sprintRepository = repository;
            _taskProjectRepository = taskProjectRepository;
            _timeProvider = timeProvider;
            _sprintMeetingLogs = sprintMeetingLogs;
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
                //create sprint meeting logs
                await _sprintMeetingLogs.CreateSprintMetting(SpringId);

                // new next sprint
                var nextSprint = await _sprintRepository.GetNextSprint(sprint.ProjectId, sprint.EndDate);
                    Sprint newSprint;
                    if(nextSprint == null)
                    {
                    newSprint = new Sprint
                    {
                        ProjectId = sprint.ProjectId,
                        Name = "Sprint " + _timeProvider.Now.ToString("ddMMyy"),
                        Description = "Next sprint after " + sprint.Name,
                        StartDate = sprint.EndDate, // kế tiếp
                        EndDate = sprint.EndDate.AddDays(14),
                        IsActive = true,
                        Status = SprintStatus.NotStarted,
                    };
                    await _sprintRepository.CreateSprintAsync(newSprint);
                    }
                    else
                    {
                        newSprint = nextSprint;
                    }
                    // update current sprint
                    var tasks = await _taskProjectRepository.GetListTasksUnFinishBySprintsIdsAsync(SpringId);
                    foreach (var task in tasks)
                    {
                        task.SprintId = newSprint.Id;
                        task.Note = (task.Note ?? "") + $" [{_timeProvider.Now}] End sprint: {sprint.Name}"+" ;";
                    }
                    await _taskProjectRepository.UpdateListTaskAsync(tasks);
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

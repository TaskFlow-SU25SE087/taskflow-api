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
        private readonly ILogProjectService _logService;

        public SprintService(ISprintRepository repository, ITaskProjectRepository taskProjectRepository,
            AppTimeProvider timeProvider, ISprintMeetingLogsService sprintMeetingLogs,
            ILogProjectService logService)
        {
            _sprintRepository = repository;
            _taskProjectRepository = taskProjectRepository;
            _timeProvider = timeProvider;
            _sprintMeetingLogs = sprintMeetingLogs;
            _logService = logService;
        }

        public async Task AddTasksToSprint(Guid ProjectId, Guid SprintId, List<Guid> TaskID, Guid memberId)
        {
            List<TaskProject> tasks = await _taskProjectRepository.GetListTasksByIdsAsync(TaskID);
            foreach (var task in tasks)
            {
                task.SprintId = SprintId;
            }
            await _taskProjectRepository.UpdateListTaskAsync(tasks);
            //log add task to sprint
            await _logService.LogAddTaskToSprint(memberId, SprintId, tasks);
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
                if (nextSprint == null)
                {
                    newSprint = new Sprint
                    {
                        ProjectId = sprint.ProjectId,
                        Name = "Sprint " + _timeProvider.Now.ToString("ddMMyy"),
                        Description = "Next sprint after " + sprint.Name,
                        StartDate = sprint.EndDate,
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
                var tasks = await _taskProjectRepository.GetListTasksUnFinishBySprintsIdsAsync(SpringId, sprint.ProjectId);
                foreach (var task in tasks)
                {
                    task.SprintId = newSprint.Id;
                    task.Note = (task.Note ?? "") + $" [{_timeProvider.Now}] End sprint: {sprint.Name}" + " ;";
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
            //log create sprint
            await _logService.LogCreateSprint(newSprint.Id);
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

        public async Task<bool> UpdateSprint(Guid ProjectId, Guid actorMemberId, Guid SprintId, UpdateSprintRequest request)
        {
            var sprint = await _sprintRepository.GetSprintByIdAsync(SprintId);
            if (sprint == null || sprint.ProjectId != ProjectId || sprint.Status == SprintStatus.Completed)
            {
                // Sprint not found or Project mismatch
                throw new AppException(ErrorCode.CannotUpdateSprint);
            }

            //update sprint and create log
            if ( sprint.ProjectId != ProjectId)
            {
                sprint!.Name = request.Name;
                await _logService.UpdateTitleSprint(sprint.Id, actorMemberId, ChangedField.Name ,sprint.Name, request.Name);
            }
            if (sprint.Description != request.Description)
            {
                sprint!.Description = request.Description;
                await _logService.UpdateTitleSprint(sprint.Id, actorMemberId, ChangedField.Description, sprint.Description, request.Description);
            }
            if (sprint.StartDate != request.StartDate)
            {
                sprint!.StartDate = request.StartDate;
                await _logService.UpdateTitleSprint(sprint.Id, actorMemberId, ChangedField.StartDate, sprint.StartDate.ToString("yyyy-MM-dd"), request.StartDate.ToString("yyyy-MM-dd"));
            }
            if (sprint.EndDate != request.EndDate)
            {
                sprint!.EndDate = request.EndDate;
                await _logService.UpdateTitleSprint(sprint.Id, actorMemberId, ChangedField.EndDate, sprint.EndDate.ToString("yyyy-MM-dd"), request.EndDate.ToString("yyyy-MM-dd"));
            }
            if (sprint.Status != request.Status)
            {
                sprint.Status = request.Status;
                await _logService.UpdateTitleSprint(sprint.Id, actorMemberId, ChangedField.Status, sprint.Status.ToString(), request.Status.ToString());
            }
            await _sprintRepository.UpdateSprintAsync(sprint);
            return true;
        }

        public async Task<SprintSummaryReportResponse?> GetSprintSummaryReport(Guid ProjectId, Guid SprintId)
        {
            // Get sprint information
            var sprint = await _sprintRepository.GetSprintByIdAsync(SprintId);
            if (sprint == null || sprint.ProjectId != ProjectId)
            {
                return null;
            }

            // Get all tasks in the sprint with board information
            var sprintTasks = await _sprintRepository.GetSprintTasksWithBoardInfo(SprintId, ProjectId);

            var report = new SprintSummaryReportResponse
            {
                Id = sprint.Id,
                Name = sprint.Name,
                Description = sprint.Description,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                Status = sprint.Status,
                TotalTasksPlanned = sprintTasks.Count,
                TasksCompleted = sprintTasks.Count(t => t.BoardType == BoardType.Done),
                TasksInProgress = sprintTasks.Count(t => t.BoardType == BoardType.InProgress),
                TasksNotStarted = sprintTasks.Count(t => t.BoardType == BoardType.Todo),
                CarryoverTasks = sprintTasks.Count(t => t.BoardType != BoardType.Done)
            };

            // Get completed tasks
            report.CompletedTasks = sprintTasks
                .Where(t => t.BoardType == BoardType.Done)
                .Select(t => new TaskSummaryItem
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    Deadline = t.Deadline,
                    Status = t.BoardName,
                    Assignees = t.Assignees
                })
                .ToList();

            // Get carryover tasks (unfinished tasks)
            report.CarryoverTasksList = sprintTasks
                .Where(t => t.BoardType != BoardType.Done)
                .Select(t => new TaskSummaryItem
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    Deadline = t.Deadline,
                    Status = t.BoardName,
                    Assignees = t.Assignees
                })
                .ToList();

            return report;
        }

    }
}

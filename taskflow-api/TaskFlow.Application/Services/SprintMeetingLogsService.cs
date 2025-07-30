using System;
using System.Threading.Tasks;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Repository;
using taskflow_api.TaskFlow.Shared.Helpers;
using System.Text.Json;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Shared.Exceptions;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using System.Collections.Generic;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class SprintMeetingLogsService : ISprintMeetingLogsService
    {
        private readonly AppTimeProvider _timeProvider;
        private readonly ISprintRepository _sprintRepository;
        private readonly ISprintMeetingLogsRepository _sprintMeetingLogsRepository;
        private readonly ITaskAssigneeRepository _taskAssigneeRepository;

        public SprintMeetingLogsService(AppTimeProvider timeProvider, ISprintRepository sprintRepository,
            ISprintMeetingLogsRepository sprintMeetingLogsRepository, ITaskAssigneeRepository taskAssigneeRepository)
        {
            _timeProvider = timeProvider;
            _sprintRepository = sprintRepository;
            _sprintMeetingLogsRepository = sprintMeetingLogsRepository;
            _taskAssigneeRepository = taskAssigneeRepository;
        }
        public async Task CreateSprintMetting(Guid SprintId)
        {
            var sprint = await _sprintRepository.GetSprintByIdAsync(SprintId);
            var taskCompletes = await _sprintRepository.GetTaskCompletes(SprintId, sprint!.ProjectId);
            var unfinishedTasks = await _sprintRepository.GetUnFinishTasks(SprintId, sprint!.ProjectId);
            var sprintMeetingLog = new SprintMeetingLog
            {
                SprintId = SprintId,
                CompletedTasksJson = JsonSerializer.Serialize(taskCompletes),
                UnfinishedTasksJson = JsonSerializer.Serialize(unfinishedTasks),
                NextPlan = string.Empty,
                CreatedAt = _timeProvider.Now,
                UpdatedAt = _timeProvider.Now
            };
            await _sprintMeetingLogsRepository.CreateMetting(sprintMeetingLog);
        }

        public async Task<List<SprintMeetingResponse>> GetAllSprintMetting(Guid projectId)
        {
            return await _sprintMeetingLogsRepository.GetAllSprintMetting(projectId);
        }

        public async Task<List<UnfinishedTaskResponse>> ListMyUpdatableUnfinished(Guid projectId, Guid projectMemberId, Guid? nextCursor)
        {
            //sprint meeting can update if it is created within 3 days
            var threshold = _timeProvider.Now.AddDays(-3);
            var sprintMeetingCanUpdate = await _sprintMeetingLogsRepository.GetAllSprintMettingCanUpdate(projectId, threshold);

            //list unfinished task
            var ufsTask = new List<UnfinishedTaskResponse>();
            foreach (var meeting in sprintMeetingCanUpdate)
            {
                var unfinishedTasks = JsonSerializer.Deserialize<List<UnfinishedTaskResponse>>(meeting.UnfinishedTasksJson);
                if (unfinishedTasks != null)
                {
                    foreach (var task in unfinishedTasks)
                    {
                        task.SprintMeetingId = meeting.Id;
                        task.SprintName = meeting.Sprint.Name;
                        task.UpdateDeadline = meeting.UpdatedAt.Value.AddDays(3);
                        ufsTask.Add(task);
                    }
                }
            }
            // user to do task can update reson
            var result = await _taskAssigneeRepository.GetTaskCanUpdateSprintMeeting(ufsTask, projectMemberId);
            return result;
        }

        public async Task<SprintMettingDetailResponse> sprintMettingDetail(Guid mettingID)
        {
            var sprintMeeting = await _sprintMeetingLogsRepository.GetSprintMettingByID(mettingID);
            if (sprintMeeting == null)
            {
                throw new AppException(ErrorCode.SprintMeetingNotFound);
            }
            //complete tasks
            var completedTasks = JsonSerializer.Deserialize<List<TaskCompleteDTO>>(sprintMeeting.CompletedTasksJson);


            //unfinished tasks
            var unfinishedTasks = JsonSerializer.Deserialize<List<UnfinishedTaskDto>>(sprintMeeting.UnfinishedTasksJson);
           
            return new SprintMettingDetailResponse
            {
                Id = sprintMeeting.Id,
                SprintId = sprintMeeting.SprintId,
                SprintName = sprintMeeting.Sprint.Name,
                CompletedTasks = completedTasks,
                UnfinishedTasks = unfinishedTasks,
                NextPlan = sprintMeeting.NextPlan,
                CreatedAt = sprintMeeting.CreatedAt,
                UpdatedAt = sprintMeeting.UpdatedAt
            };
        }

        public Task UpdateNextPlan(Guid mettingID, string nextPlan)
        {
            //sprint meeting can update if it is created within 3 days
            var threshold = _timeProvider.Now.AddDays(-3);
            var sprintmeeting = _sprintMeetingLogsRepository.GetSprintMettingByID(mettingID);
            if (sprintmeeting == null || sprintmeeting.Result.CreatedAt < threshold)
            {
                throw new AppException(ErrorCode.SprintMeetingCannotUpdate);
            }
            // update next plan
            sprintmeeting.Result.NextPlan = nextPlan;
            sprintmeeting.Result.UpdatedAt = _timeProvider.Now;
            return _sprintMeetingLogsRepository.UpdateSprintMeetingLog(sprintmeeting.Result);
        }

        public async Task<string> UpdateResonTask(Guid mettingID, Guid taskId, Guid projectMemberId, int itemVersion, string reason)
        {
            //sprint meeting can update if it is created within 3 days
            var threshold = _timeProvider.Now.AddDays(-3);
            var sprintmeeting = await _sprintMeetingLogsRepository.GetSprintMettingByID(mettingID);
            if (sprintmeeting == null || sprintmeeting.CreatedAt < threshold)
            {
                throw new AppException(ErrorCode.SprintMeetingCannotUpdate);
            }
            var unfinishedTasks = JsonSerializer.Deserialize<List<UnfinishedTaskResponse>>(sprintmeeting.UnfinishedTasksJson);
            if (unfinishedTasks == null || !unfinishedTasks.Any(x => x.Id == taskId))
            {
                throw new AppException(ErrorCode.SprintMeetingTaskNotFound);
            }
            // update reason
            var task = unfinishedTasks.First(x => x.Id == taskId);
            if (task.ItemVersion != itemVersion)
            {
                return "Someone has updated the reason. Do you want to overwrite it? New ItemVersion: " + task.ItemVersion;
            }
            task.Reason = reason;
            task.ItemVersion++;
            // check if user is assignee of task
            var taskAssignee = await _taskAssigneeRepository.GetTaskAssigneeByTaskIdAndUserIDAsync(taskId, projectMemberId);
            if (taskAssignee == null)
            {
                throw new AppException(ErrorCode.Unauthorized);
            }

            // update sprint meeting log
            sprintmeeting.UnfinishedTasksJson = JsonSerializer.Serialize(unfinishedTasks);
            sprintmeeting.UpdatedAt = _timeProvider.Now;
            await _sprintMeetingLogsRepository.UpdateSprintMeetingLog(sprintmeeting);

            //return unfinishedTasks;
            return "update reason succesfully";
        }

        public async Task<string> UpdateSprintMeeting(UpdateSprintMettingRequest request, Guid sprintmettingID)
        {
            //sprint meeting can update if it is created within 3 days
            var threshold = _timeProvider.Now.AddDays(-3);
            var sprintmeeting = await _sprintMeetingLogsRepository.GetSprintMettingByID(sprintmettingID);
            if (sprintmeeting == null || sprintmeeting.CreatedAt < threshold)
            {
                throw new AppException(ErrorCode.SprintMeetingCannotUpdate);
            }
            //json deserialize completed tasks and unfinished tasks
            var unfinishedTasks = JsonSerializer.Deserialize<List<UnfinishedTaskDto>>(sprintmeeting.UnfinishedTasksJson);
            List<UnfinishedTaskDto> rq = new List<UnfinishedTaskDto>();
            if (request.UnfinishedTasks != null)
            {
                foreach (var item in request.UnfinishedTasks)
                {
                    rq.Add(new UnfinishedTaskDto
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Reason = item.Reason,
                        Description = item.Description,
                        ItemVersion = item.ItemVersion,
                        Priority = item.Priority,
                    });
                }
            }

            // update unfinished tasks
            foreach (var task in rq)
            {
                var existingTask = unfinishedTasks.FirstOrDefault(x => x.Id == task.Id);
                if (existingTask != null && existingTask.ItemVersion == task.ItemVersion)
                {
                    if (existingTask.Reason != task.Reason)
                    {
                        existingTask.Title = task.Title;
                        existingTask.Reason = task.Reason;
                        existingTask.Description = task.Description;
                        existingTask.ItemVersion = task.ItemVersion + 1;
                        existingTask.Priority = task.Priority;
                    }
                }
                else
                {
                    return "Someone has updated the reason of taskID: "+task.Id +" Do you want to overwrite it? New ItemVersion: " + existingTask.ItemVersion;
                }
            }
            sprintmeeting.UnfinishedTasksJson = JsonSerializer.Serialize(unfinishedTasks);


             //update next plan
            sprintmeeting.NextPlan = request.NextPlan;
            sprintmeeting.UpdatedAt = _timeProvider.Now;
            await _sprintMeetingLogsRepository.UpdateSprintMeetingLog(sprintmeeting);
            return "Update sprint meeting successfully";
        }
    }
}

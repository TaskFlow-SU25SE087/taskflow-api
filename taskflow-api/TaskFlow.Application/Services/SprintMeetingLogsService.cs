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

        public async Task<Object> ListMyUpdatableUnfinished(Guid projectId, Guid projectMemberId, Guid? nextCursor)
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
    }
}

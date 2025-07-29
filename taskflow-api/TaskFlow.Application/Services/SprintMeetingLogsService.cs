using System;
using System.Threading.Tasks;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Repository;
using taskflow_api.TaskFlow.Shared.Helpers;
using System.Text.Json;
using taskflow_api.TaskFlow.Application.DTOs.Response;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class SprintMeetingLogsService : ISprintMeetingLogsService
    {
        private readonly AppTimeProvider _timeProvider;
        private readonly ISprintRepository _sprintRepository;
        private readonly ISprintMeetingLogsRepository _sprintMeetingLogsRepository;

        public SprintMeetingLogsService(AppTimeProvider timeProvider, ISprintRepository sprintRepository,
            ISprintMeetingLogsRepository sprintMeetingLogsRepository)
        {
            _timeProvider = timeProvider;
            _sprintRepository = sprintRepository;
            _sprintMeetingLogsRepository = sprintMeetingLogsRepository;
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
    }
}

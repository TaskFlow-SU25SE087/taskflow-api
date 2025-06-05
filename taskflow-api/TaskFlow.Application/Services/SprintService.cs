using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class SprintService : ISprintService
    {
        private readonly ISprintRepository _sprintRepository;
        public SprintService(ISprintRepository repository)
        {
            _sprintRepository = repository;
        }
        public async Task<bool> CreateSprint(CreateSprintRequest request)
        {
            var newSprint = new Sprint
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = SprintStatus.NotStarted
            };
            await _sprintRepository.CreateSprintAsync(newSprint);
            return true;
        }
        public async Task<bool> UpdateSprint(UpdateSprintRequest request)
        {
            var UpdateSprint = new Sprint
            {
                Id = request.SprintId,
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
            };
            await _sprintRepository.UpdateSprintAsync(UpdateSprint);
            return true;
        }

    }
}

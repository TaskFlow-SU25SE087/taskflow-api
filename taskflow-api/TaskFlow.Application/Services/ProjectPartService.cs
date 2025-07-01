using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class ProjectPartService : IProjectPartService
    {
        private readonly IProjectPartRepository _projectPartRepository;

        public ProjectPartService(IProjectPartRepository projectPartRepository)
        {
            _projectPartRepository = projectPartRepository;
        }

        public async Task CreatePart(Guid ProjectId, CreateProjectPartRequest request)
        {
            var part = new Domain.Entities.ProjectPart
            {
                Name = request.Name,
                ProgrammingLanguage = request.ProgrammingLanguage,
                Framework = request.Framework,
                ProjectId = ProjectId
            };
            await _projectPartRepository.CreatePartAsync(part);
        }
    }
}

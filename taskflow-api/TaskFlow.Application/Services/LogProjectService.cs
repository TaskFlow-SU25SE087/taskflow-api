using System;
using taskflow_api.Migrations;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Repository;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class LogProjectService : ILogProjectService
    {
        private readonly ILogProjectRepository _logProjectRepository;
        private readonly AppTimeProvider _timeProvider;
        private readonly IProjectMemberRepository _projectMemberRepository;

        public LogProjectService(ILogProjectRepository logProjectRepository, AppTimeProvider timeProvider,
            IProjectMemberRepository projectMemberRepository)
        {
            _logProjectRepository = logProjectRepository;
            _timeProvider = timeProvider;
            _projectMemberRepository = projectMemberRepository;
        }

        public Task<List<ProjectLogResponse>> AllLogPrj(Guid projectId)
        {
            return _logProjectRepository.AllLogPrj(projectId);
        }

        public async Task LogCreateProject(Guid projectId, Guid projectMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(projectMemberId);
            var log = new LogProject
            {
                ProjectMemberId = projectMemberId,
                ProjectId = projectId,
                Description = member!.User.FullName + " created project",
                CreatedAt = _timeProvider.Now,
            };
            await _logProjectRepository.CreateLogProject(log);
        }
    }
}

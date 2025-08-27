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

        //get all log of project
        public Task<List<ProjectLogResponse>> AllLogPrj(Guid projectId)
        {
            return _logProjectRepository.AllLogPrj(projectId);
        }

        //---Private helper log ---
        private async Task LogSimple(Guid projectId, Guid projectMemberId, TypeLog action, string description)
        {
            var log = new LogProject
            {
                ProjectMemberId = projectMemberId,
                ProjectId = projectId,
                ActionType = action,
                Description = description,
                CreatedAt = _timeProvider.Now,
            };
            await _logProjectRepository.CreateLogProject(log);
        }

        private async Task LogChange(Guid projectId, Guid projectMemberId, TypeLog action, 
            ChangedField field, string oldValue, string newValue)
        {
            var log = new LogProject
            {
                ProjectId = projectId,
                ProjectMemberId = projectMemberId,
                ActionType = action,
                FieldChanged = field,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedAt = _timeProvider.Now
            };
            await _logProjectRepository.CreateLogProject(log);
        }

        //---Public Log ---
        public async Task LogCreateProject(Guid projectId, Guid projectMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(projectMemberId);
            await LogSimple(projectId, projectMemberId, TypeLog.CreateProject,
                $"{member!.User.FullName} created project");
        }

        public async Task LogDeleteProject(Guid projectId, Guid projectMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(projectMemberId);
            await LogSimple(projectId, projectMemberId, TypeLog.DeleteProject,
                $"{member!.User.FullName} deleted project");
        }

        public async Task LogJoinProject(Guid projectId, Guid projectMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(projectMemberId);
            await LogSimple(projectId, projectMemberId, TypeLog.JoinProject,
                $"{member!.User.FullName} participated in the project");
        }

        public async Task LogLeaveProject(Guid projectId, Guid projectMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(projectMemberId);
            await LogSimple(projectId, projectMemberId, TypeLog.LeaveProject,
                $"{member!.User.FullName} left the project");
        }

        public async Task LogRemoveMember(Guid projectId, Guid id, Guid actorMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(id);
            var actor = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(actorMemberId);
            await LogSimple(projectId, actorMemberId, TypeLog.RemoveMember,
                $"{actor!.User.FullName} removed {member!.User.FullName} from the project");
        }
    }
}

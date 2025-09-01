using AutoMapper.Execution;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using taskflow_api.Migrations;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
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
        private readonly ISprintRepository _sprintRepository;

        public LogProjectService(ILogProjectRepository logProjectRepository, AppTimeProvider timeProvider,
            IProjectMemberRepository projectMemberRepository, ISprintRepository sprintRepository)
        {
            _logProjectRepository = logProjectRepository;
            _timeProvider = timeProvider;
            _projectMemberRepository = projectMemberRepository;
            _sprintRepository = sprintRepository;
        }

        //get all log of project
        public async Task<PagedResult<ProjectLogResponse>> AllLogPrj(Guid projectId, Guid? nextLogId)
        {
            var pagingParams = new PagingParams
            {
                PageNumber = 1,
                PageSize = 5
            };

            var item = await _logProjectRepository.AllLogPrj(projectId, nextLogId, pagingParams.PageSize);

            var pagedResult = new PagedResult<ProjectLogResponse>
            {
                Items = item,
                HasMore = item.Count == pagingParams.PageSize,
                NextCursor = item.Count == 0 ? null : item.Last().Id.ToString()
            };
            return pagedResult;
        }

        //---Private helper log ---
        private async Task LogSimple(Guid projectId, Guid projectMemberId, TypeLog action, string description, LogChangeContext logChange)
        {
            var log = new LogProject
            {
                ProjectMemberId = projectMemberId,
                ProjectId = projectId,
                ActionType = action,
                Description = description,
                CreatedAt = _timeProvider.Now,
                SprintId = logChange.SprintId,
                TaskProjectID = logChange.TaskProjectID
            };
            await _logProjectRepository.CreateLogProject(log);
        }

        private async Task LogChange(Guid projectId, Guid projectMemberId, TypeLog action, 
            ChangedField field, string oldValue, string newValue,
            LogChangeContext logChange)
        {
            var log = new LogProject
            {
                ProjectId = projectId,
                ProjectMemberId = projectMemberId,
                ActionType = action,
                FieldChanged = field,
                OldValue = oldValue,
                NewValue = newValue,
                SprintId = logChange.SprintId,
                TaskProjectID = logChange.TaskProjectID,
                CreatedAt = _timeProvider.Now
            };
            await _logProjectRepository.CreateLogProject(log);
        }

        //---Public Log ---
        public async Task LogCreateProject(Guid projectId, Guid projectMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(projectMemberId);
            await LogSimple(projectId, projectMemberId, TypeLog.CreateProject,
                $"{member!.User.FullName} created project", new LogChangeContext());
        }

        public async Task LogDeleteProject(Guid projectId, Guid projectMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(projectMemberId);
            await LogSimple(projectId, projectMemberId, TypeLog.DeleteProject,
                $"{member!.User.FullName} deleted project", new LogChangeContext());
        }

        public async Task LogJoinProject(Guid projectId, Guid projectMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(projectMemberId);
            await LogSimple(projectId, projectMemberId, TypeLog.JoinProject,
                $"{member!.User.FullName} participated in the project", new LogChangeContext());
        }

        public async Task LogLeaveProject(Guid projectId, Guid projectMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(projectMemberId);
            await LogSimple(projectId, projectMemberId, TypeLog.LeaveProject,
                $"{member!.User.FullName} left the project", new LogChangeContext());
        }

        public async Task LogRemoveMember(Guid projectId, Guid id, Guid actorMemberId)
        {
            var member = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(id);
            var actor = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(actorMemberId);
            await LogSimple(projectId, actorMemberId, TypeLog.RemoveMember,
                $"{actor!.User.FullName} removed {member!.User.FullName} from the project", new LogChangeContext());
        }

        public async Task LogCreateSprint(Guid sprintId)
        {
            var sprint = await _sprintRepository.GetSprintByIdAsync(sprintId);
            if (sprint != null)
            {
                var member = await _projectMemberRepository.FindLeader(sprint.ProjectId);
                if (member != null)
                {
                    await LogSimple(sprint.ProjectId, member.Id, TypeLog.CreateSprint,
                        $"{member.User.FullName} created sprint {sprint.Name}", new LogChangeContext());
                }
            }
        }

        public async Task UpdateTitleSprint(Guid sprintId, Guid actorMemberId, ChangedField field , string oldValue, string newValue)
        {
            var sprint = await _sprintRepository.GetSprintByIdAsync(sprintId);
            var logChange = new LogChangeContext
            {
                SprintId = sprintId,
            };
            await LogChange(sprint.ProjectId, actorMemberId, TypeLog.UpdateSprint,
                field, oldValue, newValue, logChange);
        }

        public async Task LogAddTaskToSprint(Guid actorMemberId, Guid sprintId, List<TaskProject> tasks)
        {
            var sprint = await _sprintRepository.GetSprintByIdAsync(sprintId);
            var actor = await _projectMemberRepository.FindMemberInProjectByProjectMemberID(actorMemberId);
            if (sprint == null || actor == null) return;
            var nameTask = new List<string>();
            foreach (var task in tasks)
            {
                nameTask.Add(task.Title);
            }
            await LogSimple(sprint.ProjectId, actor.Id, TypeLog.AddTaskToSprint,
                        $"Added tasks: {string.Join(", ", nameTask)} to sprint {sprint.Name}", new LogChangeContext
                        {
                            SprintId = sprintId,
                        });
        }

        public async Task CreateRessonTaskLog(Guid projectId, Guid taskId, Guid actorMemberId, string reason)
        {
            await LogSimple(projectId, actorMemberId, TypeLog.UpdateResson,
                reason, new LogChangeContext
                {
                    TaskProjectID = taskId,
                });
        }
    }
}

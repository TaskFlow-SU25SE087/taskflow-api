using Microsoft.EntityFrameworkCore;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly ITermRepository _termRepository;

        public TeamService(
            IProjectRepository projectRepository,
            IProjectMemberRepository projectMemberRepository,
            ITermRepository termRepository)
        {
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
            _termRepository = termRepository;
        }

        public async Task<List<TeamResponse>> GetTeamsByTermAsync(Guid termId)
        {
            // Verify term exists
            var term = await _termRepository.GetTermByIdAsync(termId);
            if (term == null)
            {
                throw new AppException(ErrorCode.TermNotFound);
            }

            // Get projects by term
            var projects = await _projectRepository.GetProjectsByTermAsync(termId);
            
            var teams = new List<TeamResponse>();
            
            foreach (var project in projects)
            {
                var team = await BuildTeamResponseAsync(project);
                teams.Add(team);
            }

            return teams;
        }

        public async Task<TeamResponse?> GetTeamByIdAsync(Guid projectId)
        {
            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            if (project == null)
            {
                return null;
            }

            // Convert Project entity to ProjectsResponse DTO
            var projectResponse = new ProjectsResponse
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                LastUpdate = project.LastUpdate,
                Role = null,
                Semester = project.Semester,
                TermId = project.TermId,
                TermName = project.Term?.Season + " " + project.Term?.Year,
                CreatedAt = project.CreatedAt,
                IsActive = project.IsActive
            };

            return await BuildTeamResponseAsync(projectResponse);
        }

        public async Task<List<TeamResponse>> GetAllTeamsAsync()
        {
            var projects = await _projectRepository.GetAllProjectsAsync();
            
            var teams = new List<TeamResponse>();
            
            foreach (var project in projects)
            {
                var team = await BuildTeamResponseAsync(project);
                teams.Add(team);
            }

            return teams;
        }

        private async Task<TeamResponse> BuildTeamResponseAsync(ProjectsResponse project)
        {
            var members = await _projectMemberRepository.GetDetailedTeamMembersAsync(project.Id);
            var activeMembersCount = await _projectMemberRepository.GetActiveMembersCount(project.Id);

            return new TeamResponse
            {
                ProjectId = project.Id,
                ProjectTitle = project.Title,
                ProjectDescription = project.Description,
                Semester = project.Semester ?? string.Empty,
                CreatedAt = project.CreatedAt ?? DateTime.UtcNow,
                LastUpdate = project.LastUpdate,
                IsActive = project.IsActive ?? true,
                Members = members,
                TotalMembers = members.Count,
                ActiveMembers = activeMembersCount
            };
        }
    }
}

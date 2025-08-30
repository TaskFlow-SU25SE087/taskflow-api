using Microsoft.IdentityModel.Tokens;
using System;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class TaskCommentService : Interfaces.ITaskCommentService
    {
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IFileService _fileService;
        private readonly AppTimeProvider _timeProvider;

        public TaskCommentService(ITaskCommentRepository taskCommentRepository, IHttpContextAccessor httpContextAccessor,
            IProjectMemberRepository projectMemberRepository, IFileService fileService, AppTimeProvider timeProvider)
        {
            _taskCommentRepository = taskCommentRepository;
            _httpContextAccessor = httpContextAccessor;
            _projectMemberRepository = projectMemberRepository;
            _fileService = fileService;
            _timeProvider = timeProvider;
        }

        public async Task AddComentTask(Guid ProjectId, Guid TaskId, AddTaskCommentRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            var projectMember = await _projectMemberRepository.FindMemberInProject(ProjectId, new Guid(UserId!));
            if (projectMember == null)
            {
                throw new AppException(ErrorCode.NoPermission);
            }
            var newTaskComment = new TaskComment
            {
                TaskId = TaskId,
                CommenterId =projectMember.Id,
                Content = request.Content,
                CreateAt = _timeProvider.Now,
                LastUpdatedAt = _timeProvider.Now
            };
            // check file 
            if (request.Files != null && request.Files.Any())
            {
                var urls = new List<string>();
                foreach (var file in request.Files)
                {
                    var fileUrl = await _fileService.UploadAutoAsync(file);
                    urls.Add(fileUrl);
                }
                // save file urls to the comment
                newTaskComment.AttachmentUrlsList = urls;
            }
            await _taskCommentRepository.AddTaskCommentAysc(newTaskComment);
        }
    }
}

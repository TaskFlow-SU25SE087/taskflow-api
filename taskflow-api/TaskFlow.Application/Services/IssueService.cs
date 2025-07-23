using System.Threading.Tasks;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class IssueService : IIssueService
    {
        private readonly ITaskIssueRepository _issueRepository;
        private readonly IFileService _fileService;
        private readonly ITaskAssigneeRepository _taskAssigneeRepository;
        private readonly AppTimeProvider _timeProvider;

        public IssueService(ITaskIssueRepository issueRepository, IFileService fileService,
            ITaskAssigneeRepository taskAssigneeRepository, AppTimeProvider timeProvider)
        {
            _issueRepository = issueRepository;
            _fileService = fileService;
            _taskAssigneeRepository = taskAssigneeRepository;
            _timeProvider = timeProvider;
        }
        public async Task CreateTaskIssue(Guid memberId, Guid projectId, Guid TaskId, CreateTaskIssueRequest request)
        {
            var newIssue = new Issue
            {
                CreatedBy = memberId,
                ProjectId = projectId,
                TaskProjectId = TaskId,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                Type = request.Type,
                Explanation = request.Explanation,
                Example = request.Example,
                Status = IssueStatus.Open,
                CreatedAt = _timeProvider.Now,
                IsActive = true,
            };
            //have files
            if (request.Files != null && request.Files.Any())
            {
                var urls = new List<string>();
                foreach (var file in request.Files)
                {
                    var fileUrl = await _fileService.UploadAutoAsync(file);
                    urls.Add(fileUrl);
                }
                // save file urls to the task
                newIssue!.IssueAttachmentUrlsList = urls;
            }
                //save Issue
                await _issueRepository.CreateTaskIssueAsync(newIssue);
                if (request.Type == TypeIssue.Bug)
                {
                    //Get list of members doing task   
                    var listMemberInTask = await _taskAssigneeRepository
                            .taskAssigneesAsync(TaskId);
                    var lissTaskAssignee = new List<TaskAssignee>();
                    // if issue is bug, set it as active for the person doing the task
                    foreach (var taskAginee in listMemberInTask)
                    {
                        lissTaskAssignee.Add(new TaskAssignee
                        {
                            AssignerId = memberId,
                            ImplementerId = taskAginee.ImplementerId,
                            Type = RefType.Issue,
                            RefId = TaskId,
                            IsActive = true
                        });
                    }
                    await _taskAssigneeRepository.CreateListTaskAssignee(lissTaskAssignee);
                }
        }

        public async Task<List<IssueDetailResponse>> FilterIssue(Guid projectId, IssueStatus? status, 
            TypeIssue? type, TaskPriority? priority)
        {
            return await _issueRepository.GetIssue(projectId, status, type, priority);
        }

        public Task<List<IssueDetailResponse>> GetAllIssue(Guid ProjectId)
        {
            return _issueRepository.GetIssue(ProjectId);
        }
    }
}

﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Repository;
using taskflow_api.TaskFlow.Shared.Exceptions;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class ProjectPartService : IProjectPartService
    {
        private readonly IProjectPartRepository _projectPartRepository;
        private readonly IGitHubRepoService _repoService;
        private readonly AppSetting _appSetting;
        private readonly string _BaseUrl;
        private readonly ILogger<ProjectPartService> _logger;
        private readonly ICodeScanService _codeScanService;
        private readonly ICommitRecordRepository _commitRecordRepository;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IUserGitHubRepository _userGitHubRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICommitScanIssueRepository _commitScanIssueRepository;
        private readonly IGitMemberRepository _gitMemberRepository;
        private readonly AppTimeProvider _timeProvider;
        private int PageSizeCommit = 10;

        public ProjectPartService(IProjectPartRepository projectPartRepository, IGitHubRepoService repoService,
            IOptions<AppSetting> appSetting, ILogger<ProjectPartService> logger,
            ICodeScanService codeScanService, ICommitRecordRepository commitRecordRepository,
            IRabbitMQService rabbitMQService, IUserGitHubRepository userGitHubRepository,
            IHttpContextAccessor httpContextAccessor, ICommitScanIssueRepository commitScanIssueRepository,
            IGitMemberRepository gitMemberRepository, AppTimeProvider timeProvider)
        {
            _projectPartRepository = projectPartRepository;
            _repoService = repoService;
            _appSetting = appSetting.Value;
            _BaseUrl = appSetting.Value.BaseUrl!;
            _logger = logger;
            _codeScanService = codeScanService;
            _commitRecordRepository = commitRecordRepository;
            _rabbitMQService = rabbitMQService;
            _userGitHubRepository = userGitHubRepository;
            _httpContextAccessor = httpContextAccessor;
            _commitScanIssueRepository = commitScanIssueRepository;
            _gitMemberRepository = gitMemberRepository;
            _timeProvider = timeProvider;
        }

        public async Task ConnectRepo(Guid partId, ConnectRepoRequest request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.User.FindFirst("id")?.Value;
            //get UserGitHub 
            var userGitHub = await _userGitHubRepository.GetTokenByUserIdAsync(Guid.Parse(UserId!));
            if (userGitHub == null)
            {
                throw new AppException(ErrorCode.UserGitHubTokenNotFound);
            }
            //test connect repository 
            var valid = await _repoService.TestConnection(request.RepoUrl, userGitHub!.AccessToken);
            if (!valid) throw new AppException(ErrorCode.InvalidRepoOrToken);

            var part = await _projectPartRepository.GetPartByIdAsync(partId);
            if (part == null)
            {
                throw new AppException(ErrorCode.PartNotFound);
            }
            
            //currently supports github first
            part.RepoProvider = RepoProvider.GitHub;
            part.RepoUrl = request.RepoUrl;
            part.UserGitHubTokenId = userGitHub!.Id;

            //create webhook
            var webhookUrl = $"{_BaseUrl}/api/webhooks/github";
            var checkCreateWebhookUrl = await _repoService.CreateWebhook(request.RepoUrl, userGitHub.AccessToken, webhookUrl);
            if(!checkCreateWebhookUrl)
            {
                throw new AppException(ErrorCode.WebhookCreationFailed);
            }
            //save part
            part.WebhookUrl = webhookUrl;
            await _projectPartRepository.UpdateAsync(part);

            //Get all member join git
            var gitMembers = await _repoService.GetGitHubRepoMembers(request.RepoUrl, userGitHub.AccessToken);
            if (gitMembers == null || gitMembers.Count == 0)
            {
                _logger.LogWarning($"No members found in repository {request.RepoUrl}");
                return;
            }
            //create git member
            var gitMemberEntities = new List<GitMember>();
            foreach (var gitMember in gitMembers)
            {
                gitMemberEntities.Add(new GitMember
                {
                    ProjectPartId = partId,
                    GitName = gitMember.Name,
                    GitEmail = gitMember.Email,
                    GitAvatarUrl = gitMember.AvatarUrl,
                });
            }
            //save git member
            await _gitMemberRepository.CreateListGitMember(gitMemberEntities);
        }

        public async Task CreatePart(Guid ProjectId, CreateProjectPartRequest request)
        {
            var part = new ProjectPart
            {
                Name = request.Name,
                ProgrammingLanguage = request.ProgrammingLanguage,
                Framework = request.Framework,
                ProjectId = ProjectId
            };
            await _projectPartRepository.CreatePartAsync(part);
        }

        public async Task<List<ProjectPartResponse>> GetAllRepositories(Guid ProjectId)
        {
            var part = await _projectPartRepository.GetAllPartsByProjectIdAsync(ProjectId);
            if (part == null)
            {
                throw new AppException(ErrorCode.NoHaveRepoInProject);
            }
            return part;
        }

        public async Task<List<CommitDetailResponse>> GetCommitDetail(string commitId)
        {
            return await _commitScanIssueRepository.GetByCommitCheckResultId(commitId);
        }

        public async Task<PagedResult<CommitRecordResponse>> GetCommits(Guid partId, int page)
        {
            var commits = await _commitRecordRepository.GetCommitRecordsByPartId(partId, page, PageSizeCommit);
            var countCommits = await _commitRecordRepository.CountCommitByProjectPart(partId);
            var result = new PagedResult<CommitRecordResponse>
            {
                Items = commits,
                PageNumber = page,
                PageSize = PageSizeCommit,
                TotalPages = (int)Math.Ceiling((double)countCommits / PageSizeCommit),
            };

            return result;
        }

        public async Task ProcessGitHubPushEvent(JObject payload)
        {
            if (payload == null)
            {
                _logger.LogError("Invalid payload data received for GitHub push event.");
                return;
            }

            var branchRef = payload["ref"]?.ToString();
            var repoUrl = payload["repository"]?["html_url"]?.ToString();
            var repoFullName = payload["repository"]?["full_name"]?.ToString();
            var pusher = payload["pusher"]?["name"]?.ToString();
            var commits = payload["commits"]?.ToObject<List<JObject>>();


            //get commit
            if (commits == null)
                return;

            foreach ( var commit in commits )
            {
                var message = commit["message"]?.ToString();
                var commitId = commit["id"]?.ToString();
                var timestamp = commit["timestamp"]?.ToObject<DateTime>();

                if (string.IsNullOrEmpty(commitId)) continue;
                // get repo git
                var repo = await _projectPartRepository.GetByRepoUrlAsync(repoUrl);
                if (repo == null)
                {
                    _logger.LogWarning($"Repo not found for url: {repoUrl}");
                    continue;
                }

                //check if commit already exists
                var existed = await _commitRecordRepository.ExistsByCommitId(commitId);
                if (existed)
                {
                    _logger.LogInformation($"Commit {commitId} already processed.");
                    continue;
                }
                // create commit record
                var commitRecord = new CommitRecord
                {
                    ProjectPartId = repo.Id,
                    CommitId = commitId,
                    Pusher = pusher ?? "unknown",
                    CommitMessage = message,
                    CommitUrl = $"https://github.com/{repoFullName}/commit/{commitId}",
                    PushedAt = timestamp ?? DateTime.UtcNow,
                    Status = StatusCommit.Checking,
                    ExpectedFinishAt = _timeProvider.UtcNow.AddMinutes(10),
                };

                await _commitRecordRepository.Create(commitRecord);

                //send to rabbitmq for processing
                _rabbitMQService.SendCommitJob(new CommitJobMessage
                {
                    CommitRecordId = commitRecord.Id,
                    RepoFullName = repoFullName,
                    CommitId = commitId,
                    AccessToken = repo.UserGitHubToken!.AccessToken,
                    Language = repo.ProgrammingLanguage,
                    Framework = repo.Framework,
                });

                _logger.LogInformation($"Pushed job for commit {commitId} to RabbitMQ.");
            }
        }
    }
}

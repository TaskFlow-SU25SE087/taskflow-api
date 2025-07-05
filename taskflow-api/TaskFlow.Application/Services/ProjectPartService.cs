using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class ProjectPartService : IProjectPartService
    {
        private readonly IProjectPartRepository _projectPartRepository;
        private readonly IRepoService _repoService;
        private readonly AppSetting _appSetting;
        private readonly string _BaseUrl;
        private readonly ILogger<ProjectPartService> _logger;

        public ProjectPartService(IProjectPartRepository projectPartRepository, IRepoService repoService,
            IOptions<AppSetting> appSetting, ILogger<ProjectPartService> logger)
        {
            _projectPartRepository = projectPartRepository;
            _repoService = repoService;
            _appSetting = appSetting.Value;
            _BaseUrl = appSetting.Value.BaseUrl!;
            _logger = logger;
        }

        public async Task ConnectRepo(Guid partId, ConnectRepoRequest request)
        {
            var valid = await _repoService.TestConnection(request.RepoUrl, request.AccessToken);
            if (!valid) throw new AppException(ErrorCode.InvalidRepoOrToken);

            var part = await _projectPartRepository.GetPartByIdAsync(partId);
            if (part == null)
            {
                throw new AppException(ErrorCode.PartNotFound);
            }
            //currently supports github first
            part.RepoProvider = RepoProvider.GitHub;
            part.RepoUrl = request.RepoUrl;
            part.AccessToken = request.AccessToken;

            //create webhook
            var webhookUrl = $"{_BaseUrl}/api/webhooks/github";
            var checkCreateWebhookUrl = await _repoService.CreateWebhook(request.RepoUrl, request.AccessToken, webhookUrl);
            if(!checkCreateWebhookUrl)
            {
                throw new AppException(ErrorCode.WebhookCreationFailed);
            }
            //save part
            part.WebhookUrl = webhookUrl;
            await _projectPartRepository.UpdateAsync(part);


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
            if (commits != null)
            {
                foreach (var commit in commits)
                {
                    var message = commit["message"]?.ToString();
                    var commitId = commit["id"]?.ToString();

                    if (string.IsNullOrEmpty(commitId)) continue;

                    var Repo = await _projectPartRepository.GetByRepoUrlAsync(repoUrl);
                    if (Repo == null)
                    {
                        continue;
                    }

                    //doawload file commit
                    var extractPath = await _repoService.DownloadCommitSourceAsync(repoFullName, commitId, Repo.AccessToken);

                    var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        _logger.LogInformation(file);
                    }

                    //check code


                    //delete files
                    //Directory.Delete(extractPath, true);
                }
            }
        }
    }
}

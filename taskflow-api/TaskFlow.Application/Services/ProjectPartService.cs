using Microsoft.Extensions.Options;
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

        public ProjectPartService(IProjectPartRepository projectPartRepository, IRepoService repoService,
            IOptions<AppSetting> appSetting)
        {
            _projectPartRepository = projectPartRepository;
            _repoService = repoService;
            _appSetting = appSetting.Value;
            _BaseUrl = appSetting.Value.BaseUrl!;
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
            part.Branch = request.Branch ?? "main"; //default to main 

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
    }
}

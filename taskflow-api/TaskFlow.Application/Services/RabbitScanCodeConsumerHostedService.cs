using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Runtime;
using System.Text;
using System.Text.Json;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Repository;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class RabbitScanCodeConsumerHostedService : BackgroundService
    {
        private readonly ILogger<RabbitScanCodeConsumerHostedService> _logger;
        private readonly RabbitMQSetting _settings;
        private readonly IServiceProvider _serviceProvider;

        public RabbitScanCodeConsumerHostedService(ILogger<RabbitScanCodeConsumerHostedService> logger,
            IOptions<RabbitMQSetting> settings, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _settings = settings.Value;
            _serviceProvider = serviceProvider;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _settings.ScanCommitQueue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var job = JsonSerializer.Deserialize<CommitJobMessage>(message);

                using var scope = _serviceProvider.CreateScope();
                var commitRepo = scope.ServiceProvider.GetRequiredService<ICommitRecordRepository>();
                var repoService = scope.ServiceProvider.GetRequiredService<IGitHubRepoService>();
                var codeScanService = scope.ServiceProvider.GetRequiredService<ICodeScanService>();


                if (job != null)
                {
                    var commit = await commitRepo.GetById(job.CommitRecordId);
                    if (commit != null)
                    {
                        var extractPath = await repoService
                        .DownloadCommitSourceAsync(job.RepoFullName, job.CommitId, job.AccessToken);
                        //scan code by SonarQube
                        var scanStart = DateTime.UtcNow;
                        var result = await codeScanService.ScanCommit(
                            extractPath, 
                            $"taskflow-{commit.ProjectPartId}",
                            job.Language,
                            job.Framework
                            );
                        var scanEnd = DateTime.UtcNow;
                        commit.ScanDuration = scanEnd - scanStart;

                        //save commit record
                        commit.ProjectKey = result.ProjectKey;
                        commit.OutputLog = result.OutputLog;
                        commit.ErrorLog = result.ErrorLog;
                        commit.Result = result.Success;
                        commit.ExpectedFinishAt = DateTime.UtcNow;
                        await commitRepo.Update(commit);

                        // save output check record
                        using var scopeCheck = _serviceProvider.CreateScope();

                        if (result.Success)
                        {
                            var sonarService = scope.ServiceProvider
                                    .GetRequiredService<ICodeScanService>();
                            var issues = await sonarService.GetIssuesByProjectAsync(result.ProjectKey);

                            var commitScanIssueRepo = scope.ServiceProvider
                                    .GetRequiredService<ICommitScanIssueRepository>();

                            var issueRepo = scope.ServiceProvider
                                    .GetRequiredService<ITaskIssueRepository>();
                            //get quality gate status
                            var qgStatus = await sonarService.GetQualityGateStatusAsync(result.ProjectKey);
                            commit.QualityGateStatus = qgStatus;

                            // save quality gate status
                            var metrics = await sonarService.GetProjectMeasuresAsync(result.ProjectKey);

                            //save result matrics
                            commit.Bugs = metrics.Bugs;
                            commit.Vulnerabilities = metrics.Vulnerabilities;
                            commit.CodeSmells = metrics.CodeSmells;
                            commit.SecurityHotspots = metrics.SecurityHotspots;
                            commit.DuplicatedLines = metrics.DuplicatedLines;
                            commit.DuplicatedBlocks = metrics.DuplicatedBlocks;
                            commit.DuplicatedLinesDensity = metrics.DuplicatedLinesDensity;
                            commit.Coverage = metrics.Coverage;
                            foreach (var i in issues)
                            {
                                var filePath = i.Component.Contains(":") ? i.Component.Split(':')[1] : i.Component;
                                var fullPath = Path.Combine(extractPath, filePath);
                                string lineContent = string.Empty;
                                if (File.Exists(fullPath))
                                {
                                    var lines = File.ReadAllLines(fullPath);
                                    if (i.Line > 0 && i.Line <= lines.Length)
                                    {
                                        lineContent = lines[i.Line - 1];
                                    }
                                }
                                //save result 
                                var scanIssue = new CommitScanIssue
                                {
                                    CommitRecordId = commit.Id,
                                    Rule = i.Rule,
                                    Severity = i.Severity,
                                    Message = i.Message,
                                    FilePath = i.Component.Contains(":") ? i.Component.Split(':', 2)[1] : i.Component,
                                    Line = i.Line,
                                    LineContent = lineContent,
                                    CreatedAt = DateTime.UtcNow
                                };
                                await commitScanIssueRepo.CreateAsync(scanIssue);

                                //create task issue
                                var issue = new Issue
                                {
                                    ProjectId = commit.ProjectPart.ProjectId,
                                    Title = $"{i.Severity}: {i.Message}",
                                    Description = $"File: {i.Component}, Line: {i.Line}, Rule: {i.Rule}",
                                    Priority = IssueMappingHelper.MapSeverityToPriority(i.Severity),
                                    Type = TypeIssue.FeatureRequest,
                                    IsActive = true
                                };
                                await issueRepo.CreateTaskIssueAsync(issue);
                            }
                        }

                        Directory.Delete(extractPath, true);

                        //update status commit
                        commit.Status = StatusCommit.Done;
                        commit.ResultSummary = "Scan completed via RabbitMQ.";
                        await commitRepo.Update(commit);
                    }
                }
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: _settings.ScanCommitQueue, autoAck: false, consumer: consumer);
            _logger.LogInformation("RabbitMQ consumer started and listening for messages.");
            return Task.CompletedTask;
        }
    }
}
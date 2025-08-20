using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Helpers;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class RabbitScanCodeConsumerHostedService : BackgroundService
    {
        private readonly ILogger<RabbitScanCodeConsumerHostedService> _logger;
        private readonly RabbitMQSetting _settings;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppTimeProvider _timeProvider;

        public RabbitScanCodeConsumerHostedService(
            ILogger<RabbitScanCodeConsumerHostedService> logger,
            IOptions<RabbitMQSetting> settings,
            IServiceProvider serviceProvider,
            AppTimeProvider timeProvider)
        {
            _logger = logger;
            _settings = settings.Value;
            _serviceProvider = serviceProvider;
            _timeProvider = timeProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create RabbitMQ connection and channel
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            // Declare queue
            channel.QueueDeclare(
                queue: _settings.ScanCommitQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Create consumer
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received message from RabbitMQ: {Message}", message);

                var job = JsonSerializer.Deserialize<CommitJobMessage>(message);
                if (job == null)
                {
                    _logger.LogWarning("Message deserialization returned null");
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var commitRepo = scope.ServiceProvider.GetRequiredService<ICommitRecordRepository>();
                var repoService = scope.ServiceProvider.GetRequiredService<IGitHubRepoService>();
                var codeScanService = scope.ServiceProvider.GetRequiredService<ICodeScanService>();

                var commit = await commitRepo.GetById(job.CommitRecordId);
                if (commit == null)
                {
                    _logger.LogWarning("Commit not found for id {CommitId}", job.CommitRecordId);
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                // Download commit source
                _logger.LogInformation("Starting download commit source...");
                var extractPath = await repoService
                    .CloneRepoAndCheckoutAsync(job.RepoFullName, job.CommitId, job.AccessToken);
                _logger.LogInformation("Download commit source done at path: {ExtractPath}", extractPath);

                // Start SonarQube scan
                var scanStart = DateTime.UtcNow;
                _logger.LogInformation(
                    "Starting code scan for commit {CommitId} in project {ProjectPartId}",
                    commit.CommitId,
                    commit.ProjectPartId);

                var result = await codeScanService.ScanCommit(
                    extractPath,
                    $"taskflow-{commit.ProjectPartId}-{commit.CommitId}",
                    job.Language,
                    job.Framework);

                _logger.LogInformation("SonarQube scan finished. Success: {Success}", result.Success);
                commit.ScanDuration = DateTime.UtcNow - scanStart;

                // Save commit result
                commit.ProjectKey = result.ProjectKey;
                commit.OutputLog = result.OutputLog;
                commit.ErrorLog = result.ErrorLog;
                commit.Result = result.Success;
                commit.ExpectedFinishAt = DateTime.UtcNow;

                if (result.Success)
                {
                    try
                    {


                        // Fetch issues and metrics
                        var sonarService = scope.ServiceProvider.GetRequiredService<ICodeScanService>();
                        var issues = await sonarService.GetIssuesByProjectAsync(result.ProjectKey);

                        var commitScanIssueRepo = scope.ServiceProvider.GetRequiredService<ICommitScanIssueRepository>();
                        var issueRepo = scope.ServiceProvider.GetRequiredService<ITaskIssueRepository>();

                        // Retry quality gate status if needed
                        var qgStatus = await sonarService.GetQualityGateStatusAsync(result.ProjectKey);
                        int retry = 0, maxRetry = 10, delayMs = 15000;
                        int count0Issue = 0;
                        while (retry < maxRetry)
                        {
                            qgStatus = await sonarService.GetQualityGateStatusAsync(result.ProjectKey);
                            issues = await sonarService.GetIssuesByProjectAsync(result.ProjectKey);

                            if (qgStatus != "NONE" && issues.Count > 0)
                                break;

                            if (issues.Count == 0)
                                count0Issue++;
                            else
                                count0Issue = 0;

                            if (count0Issue > 3 && qgStatus != "NONE")
                                break;

                            _logger.LogInformation(
                                "Quality Gate status is NONE or no issues, waiting {Delay}s before retry #{Retry}",
                                delayMs / 1000, retry + 1);

                            await Task.Delay(delayMs);
                            retry++;
                        }

                        commit.QualityGateStatus = qgStatus;

                        // Fetch project metrics
                        var metrics = await sonarService.GetProjectMeasuresAsync(result.ProjectKey);
                        commit.Bugs = metrics.Bugs;
                        commit.Vulnerabilities = metrics.Vulnerabilities;
                        commit.CodeSmells = metrics.CodeSmells;
                        commit.SecurityHotspots = metrics.SecurityHotspots;
                        commit.DuplicatedLines = metrics.DuplicatedLines;
                        commit.DuplicatedBlocks = metrics.DuplicatedBlocks;
                        commit.DuplicatedLinesDensity = metrics.DuplicatedLinesDensity;
                        commit.Coverage = metrics.Coverage;
                        commit.QualityScore = CommitScoreCalculator.CalculateQualityScore(commit);

                        // Process each issue
                        foreach (var i in issues)
                        {
                            string blamedEmail = string.Empty;
                            string blamedName = string.Empty;

                            var filePath = i.Component.Contains(":") ? i.Component.Split(':')[1] : i.Component;
                            var fullPath = Path.Combine(extractPath, filePath);
                            var folderName = Path.GetFileName(extractPath);
                            string cleanFilePath = filePath.StartsWith(folderName + "/") || filePath.StartsWith(folderName + "\\")
                                ? filePath.Substring(folderName.Length + 1)
                                : Path.GetFileName(filePath);

                            string lineContent = string.Empty;
                            if (File.Exists(fullPath) && i.Line > 0)
                            {
                                var lines = File.ReadAllLines(fullPath);
                                if (i.Line <= lines.Length)
                                    lineContent = lines[i.Line - 1];

                                // Get git blame info
                                try
                                {
                                    var blameProcess = new Process
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = "git",
                                            Arguments = $"blame -L {i.Line},{i.Line} --line-porcelain \"{fullPath}\"",
                                            WorkingDirectory = extractPath,
                                            RedirectStandardOutput = true,
                                            RedirectStandardError = true,
                                            UseShellExecute = false,
                                            CreateNoWindow = true
                                        }
                                    };

                                    blameProcess.Start();
                                    var output = await blameProcess.StandardOutput.ReadToEndAsync();
                                    blameProcess.WaitForExit();

                                    foreach (var line in output.Split('\n'))
                                    {
                                        if (line.StartsWith("author-mail "))
                                            blamedEmail = line.Substring("author-mail ".Length).Trim('<', '>', '\r');
                                        if (line.StartsWith("author "))
                                            blamedName = line.Substring("author ".Length).Trim();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning($"Failed to get git blame: {ex.Message}");
                                }
                            }

                            bool isDuplicate = false;
                            //bool isDuplicate = await commitRepo.checkDuplicateResult(
                            //    commit.ProjectPartId, i.Message, lineContent, blamedEmail, blamedName, cleanFilePath);

                            if (!isDuplicate)
                            {
                                // Save scan issue
                                var scanIssue = new CommitScanIssue
                                {
                                    CommitRecordId = commit.Id,
                                    Rule = i.Rule,
                                    Severity = i.Severity,
                                    Message = i.Message,
                                    FilePath = cleanFilePath,
                                    Line = i.Line,
                                    LineContent = lineContent,
                                    CreatedAt = _timeProvider.Now,
                                    BlamedGitEmail = blamedEmail,
                                    BlamedGitName = blamedName
                                };
                                await commitScanIssueRepo.CreateAsync(scanIssue);

                                // Create task issue
                                var projectMemberRepo = scope.ServiceProvider.GetRequiredService<IProjectMemberRepository>();
                                var idSystem = await projectMemberRepo.GetSystemMemberId(commit.ProjectPart.ProjectId);

                                var issue = new Issue
                                {
                                    ProjectId = commit.ProjectPart.ProjectId,
                                    Title = $"{i.Severity}: {i.Message}",
                                    Description = $"File: {i.Component}, Line: {i.Line}, Rule: {i.Rule}",
                                    Priority = IssueMappingHelper.MapSeverityToPriority(i.Severity),
                                    Type = TypeIssue.Improvement,
                                    CreatedBy = idSystem,
                                    IsActive = true,
                                    CreatedAt = _timeProvider.Now,
                                };
                                await issueRepo.CreateTaskIssueAsync(issue);

                                // Assign user to issue
                                var userRepo = scope.ServiceProvider.GetRequiredService<IGitMemberRepository>();
                                var member = await userRepo.GetMemberByNameAndEmailLocal(blamedName, blamedEmail, commit.ProjectPart.Id);

                                if (member != null)
                                {
                                    var taskAssigneeRepo = scope.ServiceProvider.GetRequiredService<ITaskAssigneeRepository>();
                                    var assigner = await projectMemberRepo.FindMemberInProject(
                                        commit.ProjectPart.ProjectId, new Guid("00000000-0000-0000-0000-000000000002"));

                                    var taskAssignee = new TaskAssignee
                                    {
                                        RefId = issue.Id,
                                        Type = RefType.Issue,
                                        AssignerId = assigner!.Id,
                                        ImplementerId = member.ProjectMemberId ?? null,
                                        CreatedAt = _timeProvider.Now,
                                    };
                                    await taskAssigneeRepo.CreateTaskAssignee(taskAssignee);
                                }
                                else
                                {
                                    _logger.LogWarning($"Member not found for name: {blamedName}, email: {blamedEmail}");
                                }
                            }
                        }
                    }
                    finally
                    {
                        // Delete Sonar project
                        await codeScanService.DeleteProjectSonar($"taskflow-{commit.ProjectPartId}-{commit.CommitId}");
                    }
                }

                // Delete extracted folder
                try
                {
                    if (Directory.Exists(extractPath))
                        Directory.Delete(extractPath, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to delete extractPath folder '{extractPath}': {ex.Message}");
                }

                // Update commit status
                commit.Status = StatusCommit.Done;
                commit.ResultSummary = "Scan completed via RabbitMQ.";
                await commitRepo.Update(commit);

               

                // Acknowledge message
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            // Start consuming messages
            channel.BasicConsume(queue: _settings.ScanCommitQueue, autoAck: false, consumer: consumer);
            _logger.LogInformation("RabbitMQ consumer started and listening for messages.");

            return Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}

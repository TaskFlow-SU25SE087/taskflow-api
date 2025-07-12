
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

                        var result = await codeScanService.ScanCommit(extractPath, $"taskflow-{commit.ProjectPartId}");

                        Directory.Delete(extractPath, true);

                        //update status commit
                        commit.Status = StatusCommit.Done;
                        commit.ResultSummary = "Scan completed via RabbitMQ.";
                        await commitRepo.Update(commit);

                        //get CommitID
                        var commitRecord = await commitRepo.GetById(job.CommitRecordId);

                        job.CommitId = commit.Id.ToString();
                        //create result commit
                        var commitResult = new CommitCheckResult
                        {
                            CommitRecordId = commitRecord!.Id,
                            Result = result.Success,
                            OutputLog = result.OutputLog,
                            ErrorLog = result.ErrorLog,
                            CreatedAt = DateTime.UtcNow
                        };
                        using var scopecheck = _serviceProvider.CreateScope();
                        var commitCheckResultRepo = scopecheck.ServiceProvider.GetRequiredService<ICommitCheckResultRepository>();

                        await commitCheckResultRepo.CreateCommitResult(commitResult);

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

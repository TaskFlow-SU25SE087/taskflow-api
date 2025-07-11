using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Runtime;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly RabbitMQSetting _settings;
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(IOptions<RabbitMQSetting> settings, ILogger<RabbitMQService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }
       
        public void SendCommitJob(CommitJobMessage job)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _settings.ScanCommitQueue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false);

            var body = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(job);

            channel.BasicPublish(exchange: "",
                         routingKey: _settings.ScanCommitQueue,
                         basicProperties: null,
                         body: body);

            _logger.LogInformation($"CommitJob sent to RabbitMQ: {job.CommitId}");
        }
    }
}


using Microsoft.Extensions.Options;
using System.Runtime;
using System;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Shared.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using System.Text.Json;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class RabbitImportFIleConsumerHostedService : BackgroundService
    {
        private readonly ILogger<RabbitImportFIleConsumerHostedService> _logger;
        private readonly RabbitMQSetting _settings;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppTimeProvider _timeProvider;

        public RabbitImportFIleConsumerHostedService(
            ILogger<RabbitImportFIleConsumerHostedService> logger,
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
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: _settings.ImportFileQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(
                queue: _settings.ImportFileQueue,
                autoAck: false,
                consumer: consumer);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received message from RabbitMQ: {Message}", message);

                var job = JsonSerializer.Deserialize<ImportFileJobMessage>(message);
                if (job == null)
                {
                    _logger.LogWarning("Message deserialization returned null");
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var rabbitMQService = scope.ServiceProvider.GetRequiredService<IRabbitMQService>();
                        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                        await userService.ImportEnrollmentsFromExcelAsync(job);

                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            };
            return Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}

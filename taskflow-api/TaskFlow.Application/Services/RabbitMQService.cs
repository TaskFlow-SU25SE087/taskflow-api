using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Runtime;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly RabbitMQSetting _settings;

        public RabbitMQService(IOptions<RabbitMQSetting> settings)
        {
            _settings = settings.Value;
        }
        public void ConnectAndSendMessage(string message)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: _settings.QueueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

            var body = System.Text.Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "",
                                 routingKey: _settings.QueueName,
                                 basicProperties: null,
                                 body: body);
            Console.WriteLine($" [x] Sent {message} to {_settings.QueueName} queue.");
        }
    }
}

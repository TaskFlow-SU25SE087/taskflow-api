namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IRabbitMQService
    {
        void ConnectAndSendMessage(string message);
    }
}

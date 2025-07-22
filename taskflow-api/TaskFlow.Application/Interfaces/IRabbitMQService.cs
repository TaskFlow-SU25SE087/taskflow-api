using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IRabbitMQService
    {
        void SendCommitJob(CommitJobMessage job);
    }
}

using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IMailService
    {
        Task SendMailAsync(MailContent content);
        Task VerifyAccount(string email, string token);
        Task SendMailJoinProject(string email, string token, string type);
        Task SendReactivationEmail(string email,string username, string fullname, string token);
        Task SendWelcomeEmail(string email, string fullname, string token);
    }
}

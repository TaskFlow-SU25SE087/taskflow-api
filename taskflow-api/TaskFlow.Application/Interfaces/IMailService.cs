using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IMailService
    {
        Task SendMailAsync(MailContent content);
        Task VerifyAccount(string email, string token);
        Task SendMailJoinProject(string email, string token, string type);
        Task SendReactivationEmail(string email, string newPass);
        Task SendWelcomeEmail(string email, string newPass);
    }
}

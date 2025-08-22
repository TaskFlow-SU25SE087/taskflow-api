using taskflow_api.TaskFlow.Application.DTOs.Request;

namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IMailService
    {
        Task SendMailAsync(MailContent content);
        Task SendTaskUpdateEmailAsync(string toEmail, string toName, string taskTitle, string updateMessage);
        Task VerifyAccount(string email, string token);
        Task SendMailJoinProject(string email, Guid ProjectId, string token, string type);
        Task SendReactivationEmail(string email,string username, string fullname, string token);
        Task SendWelcomeEmail(string email, string fullname, string token);
        Task SendResetPasswordEmail(string email, string token);
        Task SendProjectMemberChangeEmailAsync(string toEmail, string toName, string projectName, string changeMessage);
        Task SendMailNewAccount(string email, string username, string fullname, string password);
        Task SendMailReEnrollment(string email, string username, string termName, string fullName);
    }
}

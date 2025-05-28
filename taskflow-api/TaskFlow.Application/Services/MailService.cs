using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class MailService : IMailService
    {
        private readonly IMailService _mailService;
        private readonly MailSettings _mailSettings;

        public MailService(IMailService mailService, IOptions<MailSettings> mailSettings)
        {
            _mailService = mailService;
            _mailSettings = mailSettings.Value;
        }

        public async Task SendMailAsync(MailContent content)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_mailSettings.Mail, _mailSettings.DisplayName),
                Subject = content.Subject,
                Body = content.Body,
                IsBodyHtml = true,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.UTF8
            };

            mail.To.Add(content.To);
            mail.ReplyToList.Add(new MailAddress(_mailSettings.Mail));
            mail.Sender = new MailAddress(_mailSettings.Mail);

            using var smtpClient = new SmtpClient(_mailSettings.Host)
            {
                Port = _mailSettings.Port,
                Credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password),
                EnableSsl = true
            };
            try
            {
                await smtpClient.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}

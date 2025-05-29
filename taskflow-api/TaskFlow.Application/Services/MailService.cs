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
        private readonly MailSettings _mailSettings;
        private readonly AppSetting _appSetting;

        public MailService(IOptions<MailSettings> mailSettings, IOptions<AppSetting> appSetting)
        {
            _mailSettings = mailSettings.Value;
            _appSetting = appSetting.Value;
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

        public async Task SendMailJoinProject(string email, string token, string type)
        {
            var verificationUrl = $"{_appSetting.BaseUrl}/project/auth/verify-join-project?token={token}";
            var content = new MailContent
            {
                To = email,
                Subject = "Account Verification",
                Body = $"Wellcome {type} by clicking the following link: " +
                       $"<a href='{verificationUrl}' target='_blank'>{type}</a><br/><br/>" +
                       $"<span style='color: red;'>* This verification link will expire in 3 days.</span>"
            };
            try
            {
                await SendMailAsync(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending verification email: {ex.Message}");
            }
        }

        public async Task VerifyAccount(string email, string token)
        {
            var verificationUrl = $"{_appSetting.BaseUrl}/auth/verify-account?token={token}";
            var content = new MailContent
            {
                To = email,
                Subject = "Account Verification",
                Body = $"Please verify your account by clicking the following link: " +
                       $"<a href='{verificationUrl}' target='_blank'>Verify Account</a><br/><br/>" +
                       $"<span style='color: red;'>* This verification link will expire in 30 minutes.</span>"
            };
            try
            {
                await SendMailAsync(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending verification email: {ex.Message}");
            }
        }
    }
}

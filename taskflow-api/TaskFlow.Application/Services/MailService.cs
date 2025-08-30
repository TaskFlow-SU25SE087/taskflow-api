using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Net;
using System.Net.Mail;
using System.Web;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.Interfaces;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly AppSetting _appSetting;
        private readonly string _frontEndBaseUrl;

        public MailService(IOptions<MailSettings> mailSettings, IOptions<AppSetting> appSetting)
        {
            _mailSettings = mailSettings.Value;
            _appSetting = appSetting.Value;
            _frontEndBaseUrl = appSetting.Value.FrontEndBaseUrl!;
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

        public async Task SendTaskUpdateEmailAsync(string toEmail, string toName, string taskTitle, string updateMessage)
        {
            var subject = $"Update on Task: {taskTitle}";
            var body = $"Hello {toName},<br/><br/>The task '<b>{taskTitle}</b>' has been updated.<br/><br/>{updateMessage}<br/><br/>Best regards,<br/>TaskFlow Team";
            var mailContent = new MailContent
            {
                To = toEmail,
                Subject = subject,
                Body = body
            };
            await SendMailAsync(mailContent);
        }

        public async Task SendMailJoinProject(string email, Guid ProjectId, string token, string type)
        {
            var verificationUrl = $"{_appSetting.BaseUrl}/project/{ProjectId}/members/verify-join?token={token}";
            var content = new MailContent
            {
                To = email,
                Subject = "Verification Join Project",
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

        public async Task SendReactivationEmail(string email, string username, string fullname, string token)
        {
            var content = new MailContent
            {
                To = email,
                Subject = "🔐 Reactivate Your TaskFlow Account",
                Body = $@"
                        <html>
                        <head>
                        <style>
                            .container {{
                                font-family: Arial, sans-serif;
                                padding: 20px;
                                background-color: #f8f9fa;
                                border-radius: 8px;
                                color: #333;
                                max-width: 600px;
                                margin: auto;
                            }}
                            .btn {{
                                display: inline-block;
                                padding: 12px 20px;
                                margin-top: 20px;
                                background-color: #4CAF50;
                                color: white;
                                text-decoration: none;
                                border-radius: 6px;
                                font-weight: bold;
                            }}
                            .footer {{
                                margin-top: 30px;
                                font-size: 12px;
                                color: #888;
                            }}
                        </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h2>Hello {fullname},</h2>
                                <p>
                                    Your TaskFlow account for the <strong>SWP</strong> course has been reactivated.
                                </p>
                                <p>
                                    Your username is: <strong>{username}</strong>
                                </p>
                                <p>
                                    Please click the button below to set a new password and continue using your account:
                                </p>
                                <a href='{_frontEndBaseUrl}/reset-password?firstlogin=false&token={HttpUtility.UrlEncode(token)}&email={email}' class='btn'>Reset Password</a>
                                    Best regards,<br/>
                                    TaskFlow Team
                                </p>
                            </div>
                        </body>
                        </html>"
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

        public Task SendResetPasswordEmail(string email, string token)
        {
            var content = new MailContent
            {
                To = email,
                Subject = "🔐 Reset Your TaskFlow Password",
                Body = $@"
                        <html>
                        <head>
                        <style>
                            .container {{
                                font-family: Arial, sans-serif;
                                padding: 20px;
                                background-color: #f8f9fa;
                                border-radius: 8px;
                                color: #333;
                                max-width: 600px;
                                margin: auto;
                            }}
                            .btn {{
                                display: inline-block;
                                padding: 12px 20px;
                                margin-top: 20px;
                                background-color: #4CAF50;
                                color: white;
                                text-decoration: none;
                                border-radius: 6px;
                                font-weight: bold;
                            }}
                            .footer {{
                                margin-top: 30px;
                                font-size: 12px;
                                color: #888;
                            }}
                        </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h2>Hello,</h2>
                                <p>
                                    You have requested to reset your password for your TaskFlow account.
                                </p>
                                <p>
                                    Please click the button below to reset your password:
                                </p>
                                <a href='{_frontEndBaseUrl}/reset-password?token={HttpUtility.UrlEncode(token)}&email={email}' class='btn'>Reset Password</a>
                                    Best regards,<br/>
                                        Token = {token}            <br/>
                                    TaskFlow Team
                                </p>
                            </div>
                        </body>
                        </html>"
            };
            return SendMailAsync(content);
        }

        public async Task SendWelcomeEmail(string email, string fullname, string token)
        {
            var content = new MailContent
            {
                To = email,
                Subject = "🔐 Welcome to TaskFlow — Activate Your Account",
                Body = $@"
                        <html>
                        <head>
                        <style>
                            .container {{
                                font-family: Arial, sans-serif;
                                padding: 20px;
                                background-color: #f8f9fa;
                                border-radius: 8px;
                                color: #333;
                                max-width: 600px;
                                margin: auto;
                            }}
                            .btn {{
                                display: inline-block;
                                padding: 12px 20px;
                                margin-top: 20px;
                                background-color: #4CAF50;
                                color: white;
                                text-decoration: none;
                                border-radius: 6px;
                                font-weight: bold;
                            }}
                            .footer {{
                                margin-top: 30px;
                                font-size: 12px;
                                color: #888;
                            }}
                        </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h2>Hello {fullname},</h2>
                                <p>
                                    TaskFlow system has created an account for you to use in the <strong>SWP</strong> course.
                                </p>
                                <p>
                                    Please click the button below to set your new password and activate your account:
                                </p>
                                <a href='{_frontEndBaseUrl}/reset-password?firstlogin=true&token={HttpUtility.UrlEncode(token)}&email={email}' class='btn'>Reset Password</a>
                                <p class='footer'>
                                    Best regards,<br/>
                                    TaskFlow Team
                                </p>
                            </div>
                        </body>
                        </html>"
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
            var content = new MailContent
            {
                To = email,
                Subject = "🔐 Account Verification",
                Body = $@"
            <div style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
                <p>Hi,</p>
                <p>Your email verification code is:</p>
                <h2 style='color: #007bff;'>{token}</h2>
                <p>Please enter this code in the application to complete your registration.</p>
                <p style='color: red;'><strong>* This code will expire in 30 minutes.</strong></p>
                <br/>
                <p>If you did not request this, you can ignore this email.</p>
                <br/>
                <p>Thanks,<br/>Task Flow</p>
            </div>"
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
    public async Task SendProjectMemberChangeEmailAsync(string toEmail, string toName, string projectName, string changeMessage)
        {
            var subject = $"Project Member Change in {projectName}";
            var body = $"Hello {toName},<br/><br/>There has been a change in the project '<b>{projectName}</b>':<br/><br/>{changeMessage}<br/><br/>Best regards,<br/>TaskFlow Team";
            var mailContent = new MailContent
            {
                To = toEmail,
                Subject = subject,
                Body = body
            };
            await SendMailAsync(mailContent);
        }

        public Task SendMailNewAccount(string email, string username, string fullname, string password)
        {
            var content = new MailContent
            {
                To = email,
                Subject = "🎉 Welcome to TaskFlow – Your New Account is Ready!",
                Body = $@"
        <html>
        <head>
            <style>
                .btn {{
                    display: inline-block;
                    padding: 10px 20px;
                    margin-top: 15px;
                    background-color: #4CAF50;
                    color: white;
                    text-decoration: none;
                    border-radius: 6px;
                    font-weight: bold;
                }}
                .footer {{
                    margin-top: 20px;
                    font-size: 12px;
                    color: #999;
                }}
            </style>
        </head>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333'>
            <h2>Hi {fullname},</h2>
            <p>Welcome to <b>TaskFlow</b>! Your account has been successfully created.</p>
            <p>Here are your login details:</p>
            <ul>
                <li><b>Username:</b> {username}</li>
                <li><b>Password:</b> {password}</li>
            </ul>
            <p>👉 Please log in as soon as possible and change your password to keep your account secure.</p>

            <a href='{_frontEndBaseUrl}/login' class='btn' target='_blank'>Login Now</a>

            <hr/>
            <p class='footer'>This is an automated message from TaskFlow. Do not reply directly to this email.</p>
        </body>
        </html>"
            };
            return SendMailAsync(content);
        }

        public Task SendMailReEnrollment(string email, string username, string termName, string fullname)
        {
            var content = new MailContent
            {
                To = email,
                Subject = $"📚 Re-enrolled for {termName} — Your Account Details",
                Body = $@"
        <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333'>
            <h2>Hi {fullname},</h2>
            <p>Welcome back to <b>TaskFlow</b>! You have been re-enrolled for the <b>{termName}</b> term.</p>
            <p>Here are your account details:</p>
            <ul>
                <li><b>Username:</b> {username}</li>
                <li><b>Password:</b> Your existing password (if you forgot, use the Reset Password button below)</li>
            </ul>
            <p>👉 Please log in as soon as possible using your existing password. 
            If you have forgotten your password, click the link below to reset it:</p>
            <a href='{_frontEndBaseUrl}/reset-password?email={HttpUtility.UrlEncode(email)}' 
             style=""display:inline-block;
                      padding:10px 15px;
                      margin-top:10px;
                      background-color:#4CAF50;
                      color:white;
                      text-decoration:none;
                      border-radius:5px;
                      font-weight:bold;"">
               Reset Password
            </a>
            <br/><br/>
            <p>Login here: <a href='{_frontEndBaseUrl}/login'>{_frontEndBaseUrl}/login</a></p>
            <hr/>
            <p style='font-size:12px;color:#999'>This is an automated message from TaskFlow. Do not reply directly to this email.</p>
        </div>"
            };
            return SendMailAsync(content);
        }
    }
}

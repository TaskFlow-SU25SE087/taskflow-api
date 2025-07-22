using System.Net;
using System.Net.Mail;

namespace taskflow_api.TaskFlow.Shared.Helpers
{
    public class MailUtil
    {

        //send email using Gmail
        public static async Task<string> SendGmail(string _to, string _from,
                string _subject, string _body,
                string _gmail, string _password)
        {
            var mail = new MailMessage();
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;

            mail.From = new MailAddress(_from);
            mail.To.Add(_to);
            mail.Subject = _subject;
            mail.Body = _body;

            mail.ReplyToList.Add(new MailAddress(_from));
            mail.Sender = new MailAddress(_from);

            using var smptClient = new SmtpClient("smtp.gmail.com");
            smptClient.Port = 587; // Gmail SMTP port
            smptClient.EnableSsl = true; // Enable SSL
            smptClient.Credentials = new NetworkCredential(_gmail, _password);

            try
            {
                await smptClient.SendMailAsync(mail);
                return "Email sent successfully";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return $"Error sending email: {ex.Message}"; // Return error message
            }
        }

    }
}

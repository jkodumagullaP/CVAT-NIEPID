using System.Net;
using System.Net.Mail;

namespace CAT.AID.Web.Services.Notifications
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        public async Task SendAsync(string to, string subject, string body)
        {
            var mail = new MailMessage();
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            mail.From = new MailAddress("cvat@niepid.gov.in");

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(
                    "cvat@niepid.gov.in",
                    "APP_PASSWORD_HERE"),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
    }
}

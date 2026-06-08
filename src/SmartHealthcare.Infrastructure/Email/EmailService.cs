using Microsoft.Extensions.Options;
using SmartHealthcare.Application.Common.Settings;
using SmartHealthcare.Application.Contracts.Services;
using System.Net;
using System.Net.Mail;

namespace SmartHealthcare.Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings options;

        public EmailService(IOptions<EmailSettings> options)
        {
            this.options = options.Value;
        }

        public async Task SendEmailAsync(string to , string subject , string body)
        {
            using var smtpclient = new SmtpClient(options.SmtpServer, options.Port) 
            {
                   UseDefaultCredentials = false,
                   Credentials = new NetworkCredential(options.Username,options.Password),
                   EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(options.FromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await smtpclient.SendMailAsync(mailMessage);

        }
    }
}

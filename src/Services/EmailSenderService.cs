using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Schematic.Core;

namespace Schematic.BaseInfrastructure
{
    public class EmailSenderService : IEmailSenderService
    {
        protected readonly IOptionsMonitor<SchematicSettings> Settings;
        protected readonly EmailSettings EmailSettings;

        public EmailSenderService(IOptionsMonitor<SchematicSettings> settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            EmailSettings = Settings.CurrentValue.Email;
        }

        private SmtpClient GetClient()
        {
            var client = new SmtpClient();

            if (EmailSettings.SMTPCredentials != null) 
            {
                client.Credentials = EmailSettings.SMTPCredentials;
            }

            if (EmailSettings.SMTPHost.HasValue()) 
            {
                client.Host = EmailSettings.SMTPHost;
            }

            if (EmailSettings.SMTPPort > 0) 
            {
                client.Port = EmailSettings.SMTPPort.Value;
            }

            if (EmailSettings.SMTPEnableSSL) 
            {
                client.EnableSsl = true;
            }

            return client;
        }

        public Task SendEmailAsync(string email, string subject, string body)
        {
            using (var message = new MailMessage())
            {   
                message.To.Add(email);

                if (EmailSettings.FromMailAddress != null) 
                {
                    message.From = EmailSettings.FromMailAddress;
                }

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var client = GetClient())
                {
                    client.Send(message);
                }
            }

            return Task.CompletedTask;
        }
    }
}
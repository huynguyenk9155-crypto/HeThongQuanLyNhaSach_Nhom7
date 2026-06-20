using System.Net;
using System.Net.Mail;

namespace Tuan6.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var host = _configuration["Mail:Host"] ?? Environment.GetEnvironmentVariable("Mail__Host") ?? string.Empty;
            var portStr = _configuration["Mail:Port"] ?? Environment.GetEnvironmentVariable("Mail__Port") ?? "587";
            var username = _configuration["Mail:Username"] ?? Environment.GetEnvironmentVariable("Mail__Username") ?? string.Empty;
            var password = _configuration["Mail:Password"] ?? Environment.GetEnvironmentVariable("Mail__Password") ?? string.Empty;
            var senderEmail = _configuration["Mail:SenderEmail"] ?? Environment.GetEnvironmentVariable("Mail__SenderEmail") ?? username;
            var senderName = _configuration["Mail:SenderName"] ?? Environment.GetEnvironmentVariable("Mail__SenderName") ?? "BookStore Support";

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning($"SMTP settings are incomplete. Simulated email send to {toEmail} with subject '{subject}'. Body: {body}");
                return;
            }

            int.TryParse(portStr, out var port);

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(senderEmail, senderName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient(host, port))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(username, password);
                    client.EnableSsl = true;

                    await client.SendMailAsync(message);
                }
            }
        }
    }
}

using MailKit.Security;
using MimeKit;
using OnlineJobRecruitmentSystem.Services.Interfaces;

namespace OnlineJobRecruitmentSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var from = _configuration["EmailSettings:From"]!;
            var password = _configuration["EmailSettings:Password"]!;

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(from, password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}
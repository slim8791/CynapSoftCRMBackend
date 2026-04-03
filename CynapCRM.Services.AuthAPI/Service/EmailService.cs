using CynapCRM.Services.AuthAPI.Service.IService;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace CynapCRM.Services.AuthAPI.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSettings = _config.GetSection("EmailSettings"); 
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("CynapCRM", emailSettings["SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Connexion au serveur SMTP
                await client.ConnectAsync(emailSettings["SmtpServer"],
                    int.Parse(emailSettings["Port"]), SecureSocketOptions.StartTls);

                // Authentification
                await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);

                // Envoi réel
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}
using CynapCRM.Services.AuthAPI.Service.IService;
using System.Diagnostics;

namespace CynapCRM.Services.AuthAPI.Service
{
    public class EmailService : IEmailService
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Debug.WriteLine("************************************************");
            Debug.WriteLine($"ENVOI EMAIL À : {email}");
            Debug.WriteLine($"SUJET : {subject}");
            Debug.WriteLine($"CONTENU : {htmlMessage}");
            Debug.WriteLine("************************************************");

            return Task.CompletedTask;
        }
    }
}

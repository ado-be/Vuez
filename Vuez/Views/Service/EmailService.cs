using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace vuez.Services
{
    public class EmailService
    {
        private readonly string smtpServer = "smtp.gmail.com"; 
        private readonly int smtpPort = 587; 
        private readonly string smtpUser = "ado.benedik@gmail.com"; 
        private readonly string smtpPassword = "drhb xpmu ifkj vswh"; 

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("VUEZ", smtpUser));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            emailMessage.Body = new TextPart("plain")
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    Console.WriteLine("📧 Pripájam sa k SMTP serveru...");
                    await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

                    Console.WriteLine("🔑 Autentifikujem...");
                    await client.AuthenticateAsync(smtpUser, smtpPassword);

                    Console.WriteLine("📤 Odosielam e-mail...");
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                    Console.WriteLine($"✅ Email úspešne odoslaný na {toEmail}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Chyba pri odosielaní e-mailu: {ex.Message}");
                }
            }
        }
    }
}

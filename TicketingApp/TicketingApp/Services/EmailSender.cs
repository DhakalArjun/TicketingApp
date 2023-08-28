using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace TicketingApp.Services;

public class EmailSender : IEmailService
{
    
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;


    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                       ILogger<EmailSender> logger,
                       IConfiguration configuration)
    {
        Options = optionsAccessor.Value;
        _logger = logger;
        _configuration = configuration;
    }

    public AuthMessageSenderOptions Options { get; }
    
    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        using (var client = new SmtpClient())
        {
            var credentials = new NetworkCredential
            {
                UserName = "fsd07team@gmail.com",
                Password = "awufprfmveebjgep"
            };

            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;

            var emailMessage = new MailMessage
            {
                From = new MailAddress("fsd07team@gmail.com"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            emailMessage.To.Add(toEmail);

            await client.SendMailAsync(emailMessage);
        }
    }
}

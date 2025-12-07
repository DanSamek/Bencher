using Microsoft.AspNetCore.Identity;
using WebApplication.Data.Models;
using System.Net.Mail;
using System.Net;
using log4net;

namespace WebApplication.Services.GmailSender;

/// <summary>
/// Gmail sender for password reset.
/// </summary>
public class GmailSender : IEmailSender<ApplicationUser>
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(GmailSender));
    
    private const string ENV_GMAIL_ADDRESS = "GMAIL_EMAIL";
    private const string ENV_GMAIL_PASSWORD = "GMAIL_PASSWORD";
    private const string ENV_GMAIL_SUBJECT = "GMAIL_SUBJECT";
    
    private const  string DEFAULT_SUBJECT = "Bencher - reset password";
    private const string SMTP_ADDRESS = "smtp.gmail.com";

    private readonly string _gmailAddress;
    private readonly string _gmailPassword;
    private readonly string _mailSubject;
    
    /// <summary>
    /// .ctor
    /// </summary>
    public GmailSender()
    {
        _gmailAddress = Environment.GetEnvironmentVariable(ENV_GMAIL_ADDRESS) ?? throw new NullReferenceException($"Unable to read {ENV_GMAIL_ADDRESS} from environment variables.");
        _gmailPassword = Environment.GetEnvironmentVariable(ENV_GMAIL_PASSWORD) ?? throw new NullReferenceException($"Unable to read {ENV_GMAIL_PASSWORD} from environment variables.");
        _mailSubject = Environment.GetEnvironmentVariable(ENV_GMAIL_SUBJECT) ?? DEFAULT_SUBJECT;
    }

    /// <inheritdoc />
    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        await Task.Run(() =>
        {
            try
            {
                var fromAddress = new MailAddress(_gmailAddress);
                var toAddress = new MailAddress(email);
                var body = $"Password reset: <a href=\"{resetLink}\" targed=\"_blank\">click</a>";

                var smtp = new SmtpClient
                {
                    Host = SMTP_ADDRESS,
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, _gmailPassword),
                    Timeout = 20000
                };

                using var message = new MailMessage(fromAddress, toAddress);
                message.Subject = _mailSubject;
                message.IsBodyHtml = true;
                message.Body = body;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            return Task.CompletedTask;
        });
    }
    
    /// <inheritdoc />
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        => throw new NotImplementedException();
    
    /// <inheritdoc /> 
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => throw new NotImplementedException();
}
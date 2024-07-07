using ClinicPlatformDTOs.MiscModels;
using System.Net.Mail;

namespace ClinicPlatformWebAPI.Services.EmailService
{
    public interface IEmailService: IDisposable
    {
        Task<bool> SendMail(SmtpClient client, string from, string to, string subject, string body);

        Task<bool> SendMailGoogleSmtp(string from, string to, string subject, string body, string gmailsend, string gmailpassword);

        Task<bool> SendMailGoogleSmtp(EmailServiceModel configuration);

        Task<bool> SendMailGoogleSmtp(string target, string subject, string body);

        EmailServiceModel CreateConfiguration(string account, string password, string subject, string body, string target);
    }
}

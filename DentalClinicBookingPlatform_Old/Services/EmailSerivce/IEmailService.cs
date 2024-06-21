using Core.Misc;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Services.EmailSerivce
{
    public interface IEmailService
    {
        Task<bool> SendMail(SmtpClient client, string from, string to, string subject, string body);

        Task<bool> SendMailGoogleSmtp(string from, string to, string subject, string body, string gmailsend, string gmailpassword);

        Task<bool> SendMailGoogleSmtp(EmailServiceModel configuration);

        Task<bool> SendMailGoogleSmtp(string target, string subject, string body);

        EmailServiceModel CreateConfiguration(string account, string password, string subject, string body, string target);
    }
}

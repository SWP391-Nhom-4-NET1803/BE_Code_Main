using ClinicPlatformDTOs.MiscModels;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ClinicPlatformWebAPI.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private bool disposedValue;

        public EmailService(IConfiguration config)
        {
            this._configuration = config;
        }

        /// <summary>
        ///     I'm tired of repeating myself so creating this should help me a litle bit.
        /// </summary>
        /// <param name="account">The sender account email (example@gmail.com)</param>
        /// <param name="password">The sender account password</param>
        /// <param name="subject">The email subject</param>
        /// <param name="body">The email body</param>
        /// <param name="target">The recipient email</param>
        /// <returns>A configuration</returns>
        public EmailServiceModel CreateConfiguration(string account, string password, string subject, string body, string target)
        {
            return new EmailServiceModel()
            {
                account = account,
                password = password,
                target = target,
                subject = subject,
                body = body
            };
        }

        /// <summary>
        ///  The base email sender service.
        /// </summary>
        /// <param name="client">The smpt client</param>
        /// <param name="from">Our email address</param>
        /// <param name="to">Client</param>
        /// <param name="subject">The subject of email</param>
        /// <param name="body">The email body</param>
        /// <returns>True if sent successfully, else false</returns>
        public async Task<bool> SendMail(SmtpClient client, string from, string to, string subject, string body)
        {
            MailMessage emailMessage = new MailMessage(from, to, subject, body);

            emailMessage.BodyEncoding = Encoding.UTF8;
            emailMessage.SubjectEncoding = Encoding.UTF8;
            emailMessage.IsBodyHtml = true;
            emailMessage.ReplyToList.Add(new MailAddress(from));
            emailMessage.Sender = new MailAddress(from);

            try
            {
                await client.SendMailAsync(emailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // TODO: Using Exception Handler to response instead.
                return false;
            }
        }

        public async Task<bool> SendMailGoogleSmtp(string from, string to, string subject, string body, string gmailsend, string gmailpassword)
        {

            MailMessage message = new MailMessage(
                from: from,
                to: to,
                subject: subject,
                body: body
            );
            message.BodyEncoding = Encoding.UTF8;
            message.SubjectEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            message.ReplyToList.Add(new MailAddress(from));
            message.Sender = new MailAddress(from);

            // Tạo SmtpClient kết nối đến smtp.gmail.com
            using (SmtpClient client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(gmailsend, gmailpassword);
                client.EnableSsl = true;
                return await SendMail(client, from, to, subject, body);
            }

        }

        public async Task<bool> SendMailGoogleSmtp(EmailServiceModel model)
        {
            MailMessage message = new MailMessage(
                            from: model.account,
                            to: model.target,
                            subject: model.subject,
                            body: model.body
                        );
            message.BodyEncoding = Encoding.UTF8;
            message.SubjectEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            message.ReplyToList.Add(new MailAddress(model.account));
            message.Sender = new MailAddress(model.account);

            // Tạo SmtpClient kết nối đến smtp.gmail.com
            using (SmtpClient client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(model.account, model.password);
                client.EnableSsl = true;
                return await SendMail(client, model.account, model.target, model.subject, model.body);
            }

        }

        /// <summary>
        ///  The service sends an email to the target (usually for notifications and other verification purposes)
        ///  This is a much more simplified way to send an email.
        ///  Consider using this instead of other three
        /// </summary>
        /// <param name="target">The target email or the recipient</param>
        /// <param name="subject">The subject of the email that's summarize the email topic</param>
        /// <param name="body">The email body with descriptions and detailed contents</param>
        /// <returns>true if success, else false.</returns>
        public async Task<bool> SendMailGoogleSmtp(string target, string subject, string body)
        {
            var account = _configuration.GetValue<string>("EmailService:Email")!;
            var password = _configuration.GetValue<string>("EmailService:Password")!;

            MailMessage emailMessage = new MailMessage(account, target, subject, body);

            emailMessage.BodyEncoding = Encoding.UTF8;
            emailMessage.SubjectEncoding = Encoding.UTF8;
            emailMessage.IsBodyHtml = true;
            emailMessage.ReplyToList.Add(new MailAddress(account));
            emailMessage.Sender = new MailAddress(account);

            try
            {
                using (SmtpClient client = new SmtpClient("smtp.gmail.com"))
                {
                    client.Port = 587;
                    client.Credentials = new NetworkCredential(account, password);
                    client.EnableSsl = true;
                    await client.SendMailAsync(emailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

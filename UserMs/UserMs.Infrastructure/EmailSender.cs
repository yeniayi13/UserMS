using AuthMs.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UserMs.Core;
using System.Diagnostics.CodeAnalysis;

namespace UserMs.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class EmailSender: IEmailSender
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("e.store.software@gmail.com", "wjam hrmu foxr fwwu"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("e.store.software@gmail.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                //TODO: Tendra algo que ver con colas?
                throw new EmailSenderException("Error sending email", ex);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Link.Core.Utilities.Email
{

    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }


    public class MailAttachments
    {

        public byte[] File { get; set; }
        public string FileName { get; set; }
    }


    public interface IMailer
    {
        void SendEmail(string[] email, string[] Cc, string subject, string body, List<MailAttachments> attachments);
    }

    public class Mailer : IMailer
    {
        private readonly SmtpSettings _smtpSettings;
        public IConfiguration Configuration { get; }

        public Mailer(IConfiguration configuration)
        {
            Configuration = configuration;
            _smtpSettings = Configuration.GetSection("SmtpSettings").;
        }


        public void SendEmail(string[] email, string[] Cc, string subject, string body, List<MailAttachments> attachments)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            BodyBuilder _body = new BodyBuilder
            {
                HtmlBody = body
            };
            
            if (attachments != null)
            {
                foreach (var att in attachments)
                {
                    _body.Attachments.Add(att.FileName, att.File);
                }
            }

            foreach (var ccMail in Cc)
            {
                if(ccMail.Length > 0) message.Cc.Add(new MailboxAddress(ccMail, ccMail));
            }
            foreach (var mail in email)
            {
                message.To.Add(new MailboxAddress(mail, mail));
            }
            message.Subject = subject;
            message.Body = _body.ToMessageBody();

            
         
            
            using var client = new SmtpClient {ServerCertificateValidationCallback = (s, c, h, e) => true};

            client.Connect("smtp.office365.com", 587, SecureSocketOptions.StartTls);
            client.Authenticate("", "");
            
            // client.Connect(_smtpSettings.Server);

            // client.Authenticate(_smtpSettings.Username, _smtpSettings.Password);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}

using eVote360.Core.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmail(string receptor, string subject, string body)
        {
            var email = _configuration.GetValue<string>("EmailConfiguration:Email");
            var password = _configuration.GetValue<string>("EmailConfiguration:Password");
            var host = _configuration.GetValue<string>("EmailConfiguration:Host");
            var port = _configuration.GetValue<int>("EmailConfiguration:Port");

            var smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new NetworkCredential(email, password);


            var message = new MailMessage(email!, receptor)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await smtpClient.SendMailAsync(message);
        }
    }
}

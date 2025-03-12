using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JersyHub.Application.Repository.IRepository;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace JersyHub.Infrastructure.Repo
{
    public class AppEmailSender : IAppEmailSender
    {

        private readonly IConfiguration _configuration;

        public AppEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            using var client = new SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]))
            {
                Credentials = new NetworkCredential(emailSettings["SenderEmail"], emailSettings["Password"]),
                EnableSsl = true
            };

            using var message = new MailMessage(emailSettings["SenderEmail"], to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
    }


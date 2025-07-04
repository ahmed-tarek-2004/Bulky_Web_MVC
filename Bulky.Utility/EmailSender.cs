using FluentEmail.Core;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        public string SendGridSecret { get; set; }

        private readonly IFluentEmail _email;

        public EmailSender(IFluentEmail email)
        {
            _email = email;
        }
        //public EmailSender(IConfiguration _config)
        //{
        //    SendGridSecret = _config.GetValue<string>("SendGrid:SecretKey");
        //}

        public async Task SendEmailAsync(string to, string subject, string body)
        {

            await _email
          .To(to)
          .Subject(subject)
          .Body(body, isHtml: true)
          .SendAsync();

            //var client = new SendGridClient(SendGridSecret);

            //var from = new EmailAddress("ahmedzaher75802004@gmail.com", "Bulky Book");
            //var to = new EmailAddress(email);
            //var message = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            //return client.SendEmailAsync(message);
        }
    }
}

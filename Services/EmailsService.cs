using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;
using Craidd.Config;
using System;
using System.Linq;
using System.Dynamic;

namespace Craidd.Services
{
    public class EmailsService : IEmailsService
    {
        private readonly EmailsServiceOptions _options;
        private readonly ITemplatesService _templatesService;
        public EmailsService(IOptions<EmailsServiceOptions> optionsAccessor, ITemplatesService templatesService)
        {
            _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
            _options.Validate();
            _templatesService = templatesService;
        }

        public async Task<bool> sendEmailFromTemplateAsync(string email, string subject, string templateFile, Dictionary<string, object> messageData)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.fromName, _options.fromEmail));
            message.To.Add(new MailboxAddress(email));
            message.Subject = subject;

            var localMessageData = messageData.Aggregate(
                new ExpandoObject() as IDictionary<string, Object>,
                (a, p) => { a.Add(p.Key, p.Value); return a; }
            );

            string html = await _templatesService.engine.CompileRenderAsync($"Emails/{templateFile}.cshtml", localMessageData);
            message.Body = new TextPart("html")
            {
                Text = html
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_options.host, _options.port, _options.useSSL);
                    await client.AuthenticateAsync(_options.username, _options.password);
                    await client.SendAsync(message);
                }
                catch (SmtpCommandException ex)
                {
                    switch (ex.ErrorCode)
                    {
                        case SmtpErrorCode.RecipientNotAccepted:
                            Console.WriteLine("\tRecipient not accepted: {0}", ex.Mailbox);
                            break;
                        case SmtpErrorCode.SenderNotAccepted:
                            Console.WriteLine("\tSender not accepted: {0}", ex.Mailbox);
                            break;
                        case SmtpErrorCode.MessageNotAccepted:
                            Console.WriteLine("\tMessage not accepted.");
                            break;
                    }

                    return false;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }

            return true;
        }
    }
}
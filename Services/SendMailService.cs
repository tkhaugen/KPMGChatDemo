using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SimpleEchoBot.Services
{
    public interface ISendMailService
    {
        Task SendMail(string sender, string recipient, string subject, string body);
        Task SendMail(string sender, string[] recipients, string subject, string body);
        SendGridClient CreateClient();
    }

    public class SendMailService : ISendMailService
    {
        private static ISendMailService _instance;

        public static ISendMailService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SendMailService();
                }
                return _instance;
            }
        }

        public async Task SendMail(string sender, string recipient, string subject, string body)
        {
            await SendMail(sender, new[] { recipient }, subject, body);
        }

        public async Task SendMail(string sender, string[] recipients, string subject, string body)
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(sender),
                Subject = subject,
                PlainTextContent = body,
                HtmlContent = HtmlizeText(body)
            };
            foreach (var recipient in recipients)
                msg.AddTo(recipient);

            await CreateClient().SendEmailAsync(msg);
        }

        public SendGridClient CreateClient()
        {
            return new SendGridClient(MailConfiguration.SendGridApiKey);
        }

        private string HtmlizeText(string text)
        {
            var paragraphs =
                // Split into paragraphs
                text.Replace("\r", "").Split(new[] { "\n\n" }, StringSplitOptions.None)
                // Replace newlines inside paragraphs with <br/>
                .Select(para => para.Replace("\n", "<br/>" + Environment.NewLine))
                // Process markdown per paragraph
                .Select(para => ProcessMarkdown(para))
                // Enclose paragraphs in <p>
                .Select(para => "<p>" + para + "</p>").ToArray();

            // Join and return
            return string.Join(Environment.NewLine, paragraphs);
        }

        private string ProcessMarkdown(string text)
        {
            // Headers
            text = Regex.Replace(text, @"^# (.+)$", @"<h1>$+</h1>", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^## (.+)$", @"<h2>$+</h2>", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^### (.+)$", @"<h3>$+</h3>", RegexOptions.Multiline);

            // Strong
            text = Regex.Replace(text, @"\*\*([^\*]+?)\*\*", @"<strong>$+</strong>", RegexOptions.None);
            text = Regex.Replace(text, @"__([^_]+?)__", @"<strong>$+</strong>", RegexOptions.None);
            // Emphasis
            text = Regex.Replace(text, @"\*([^\*]+?)\*", @"<em>$+</em>", RegexOptions.None);
            text = Regex.Replace(text, @"_([^_]+?)_", @"<em>$+</em>", RegexOptions.None);

            return text;
        }
    }
}
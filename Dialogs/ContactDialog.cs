using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using SimpleEchoBot.Properties;
using System.Collections.Generic;
using Microsoft.Bot.Connector;

namespace SimpleEchoBot.Dialogs
{
    using Microsoft.Bot.Builder.FormFlow;
    using Models.CVPartner;
    using SimpleEchoBot.Helpers;
    using SimpleEchoBot.Models;
    using SimpleEchoBot.Services;

    [Serializable]
    public class ContactDialog : IDialog<object>
    {
        private const string _pcd_key_user = "ContactByUser";

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait<User>(PromptChoices);
        }

        private async Task PromptChoices(IDialogContext context, IAwaitable<User> result)
        {
            var user = await result;

            context.PrivateConversationData.SetValue(_pcd_key_user, user);

            var options = new PromptOptions<string>(
                string.Format(Resources.ContactQuestion, user.Name),
                Resources.SorryChoose,
                Resources.TooManyAttempts,
                new string[] { Resources.ContactMe, Resources.IllContact },
                1);

            PromptDialog.Choice(
                context,
                ProcessChoiceAsync,
                options);
        }

        private async Task ProcessChoiceAsync(IDialogContext context, IAwaitable<string> result)
        {
            var chosen = await result;

            if (chosen == Resources.ContactMe)
            {
                var dialog = Chain.From(() => FormDialog.FromForm(ContactDetails.BuildForm, FormOptions.PromptInStart));
                context.Call(dialog, ResumeAfterContactDetails);
            }
            else if (chosen == Resources.IllContact)
            {
                await context.PostAsync(Resources.IllContactResponse);
                context.Done(string.Empty);
            }
            else
            {
                context.Done(string.Empty);
            }
        }

        private async Task ResumeAfterContactDetails(IDialogContext context, IAwaitable<ContactDetails> result)
        {
            var details = await result;
            var user = context.PrivateConversationData.GetValue<User>(_pcd_key_user);

            await SendMessageToUser(user, details, "Sjømat");
            await SendMessageToConfiguredRecipients(user, details, "Sjømat");
            await SendReceiptToContact(user, details, "Sjømat");


            await context.PostAsync(Resources.YouWillBeContacted);
            context.Done(string.Empty);
        }

        private async Task SendMessageToUser(User user, ContactDetails contactDetails, string interest)
        {
            var message = string.Format(Resources.EmailToUserBody,
                interest,
                user.Name, user.Role, user.OfficeName, user.Email, user.Telephone,
                contactDetails.Name, contactDetails.Company, contactDetails.Email, contactDetails.Phone);

            //await SendMailService.Instance.SendMail(MailConfiguration.Sender, StringHelper.MakeEmailAddress(user.Name, user.Email), Resources.EmailToUserSubject, message);
            await SendMailService.Instance.SendMail(MailConfiguration.Sender, MailConfiguration.Recipients, Resources.EmailToUserSubject, message);
        }

        private async Task SendMessageToConfiguredRecipients(User user, ContactDetails contactDetails, string interest)
        {
            var message = string.Format(Resources.EmailToConfiguredRecipientsBody,
                interest,
                user.Name, user.Role, user.OfficeName, user.Email, user.Telephone,
                contactDetails.Name, contactDetails.Company, contactDetails.Email, contactDetails.Phone);

            await SendMailService.Instance.SendMail(MailConfiguration.Sender, MailConfiguration.Recipients, Resources.EmailToConfiguredRecipientsSubject, message);
        }

        private async Task SendReceiptToContact(User user, ContactDetails contactDetails, string interest)
        {
            var message = string.Format(Resources.EmailReceiptToContactBody,
                interest,
                user.Name, user.Role, user.OfficeName, user.Email, user.Telephone,
                contactDetails.Name, contactDetails.Company, contactDetails.Email, contactDetails.Phone);

            //await SendMailService.Instance.SendMail(MailConfiguration.Sender, StringHelper.MakeEmailAddress(contactDetails.Name, contactDetails.Email), Resources.EmailReceiptToContactSubject, message);
            await SendMailService.Instance.SendMail(MailConfiguration.Sender, MailConfiguration.Recipients, Resources.EmailReceiptToContactSubject, message);
        }

    }
}
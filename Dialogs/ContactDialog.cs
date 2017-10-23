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
    using SimpleEchoBot.Models;

    [Serializable]
    public class ContactDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait<User>(PromptChoices);
        }

        private async Task PromptChoices(IDialogContext context, IAwaitable<User> result)
        {
            var user = await result;

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
            await context.PostAsync(Resources.YouWillBeContacted);
            context.Done(string.Empty);
        }
    }
}
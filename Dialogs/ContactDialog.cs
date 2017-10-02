using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using SimpleEchoBot.Properties;
using System.Collections.Generic;
using Microsoft.Bot.Connector;

namespace SimpleEchoBot.Dialogs
{
    using Models.CVPartner;

    [Serializable]
    public class ContactDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait<User>(PromptChoices);
        }

        private async Task PromptChoices(IDialogContext context, IAwaitable<User> result)
        {
            try
            {
                var user = await result;

                PromptDialog.Choice(
                    context,
                    ProcessChoiceAsync,
                    new string[] { Resources.ContactMe, Resources.IllContact },
                    string.Format(Resources.ContactQuestion, user.Name),
                    Resources.SorryChoose,
                    3);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task ProcessChoiceAsync(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var chosen = await result;

                var actions = new Dictionary<string, Action>
                {
                    { Resources.ContactMe, async () => await ContactMe(context) },
                    { Resources.IllContact, async () => await IllContact(context) },
                };

                Action action;

                if (actions.TryGetValue(chosen.ToString(), out action))
                {
                    action();
                    context.Done(string.Empty);
                }
                else
                {
                    context.Done(string.Empty);
                }

                //context.Wait<User>(PromptChoices);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task IllContact(IDialogContext context)
        {
            await context.PostAsync(Resources.IllContactResponse);
        }

        private async Task ContactMe(IDialogContext context)
        {
            await context.PostAsync(Resources.YouWillBeContacted);
        }
    }
}
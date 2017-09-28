using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using SimpleEchoBot.Properties;
using System.Collections.Generic;
using SimpleEchoBot.Models;
using Microsoft.Bot.Connector;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class ContactDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait<CV>(this.PromptChoices);
        }

        private async Task PromptChoices(IDialogContext context, IAwaitable<CV> result)
        {
            try
            {
                var cv = await result;

                PromptDialog.Choice(
                    context,
                    this.ChosenAsync,
                    new string[] { Resources.ContactMe, Resources.IllContact },
                    string.Format(Resources.ContactQuestion, cv.Name),
                    Resources.SorryChoose,
                    3);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task ChosenAsync(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var chosen = await result;

                var actions = new Dictionary<string, Action> {
                                { Resources.ContactMe, async() => await ContactMe(context) },
                                { Resources.IllContact, async() => await IllContact(context) },
                            };

                Action action;

                if (actions.TryGetValue(chosen.ToString(), out action))
                {
                    action();
                }
                else
                {
                    context.Done(string.Empty);
                }

                context.Wait<CV>(this.PromptChoices);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task IllContact(IDialogContext context)
        {
            await context.PostAsync(Resources.NotImplemented);
        }

        private async Task ContactMe(IDialogContext context)
        {
            await context.PostAsync(Resources.NotImplemented);
        }
    }
}
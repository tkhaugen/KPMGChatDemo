using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SimpleEchoBot.Models;
using SimpleEchoBot.Properties;
using SimpleEchoBot.Services;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class IndustryDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(Resources.IndustryChosen);

            context.Wait(PromptChoices);
        }

        private async Task PromptChoices(IDialogContext context, IAwaitable<object> result)
        {
            var industries = await CVPartnerService.Instance.GetIndustries();
            PromptDialog.Choice(
                context,
                ProcessChoice,
                industries,
                Resources.IndustryQuestion,
                Resources.SorryChoose,
                3);
        }

        private async Task ProcessChoice(IDialogContext context, IAwaitable<string> result)
        {
            var chosen = await result;
            var cvs = await CVPartnerService.Instance.GetCVs(chosen);
            var cv = cvs.First(); //TODO: Find consultant with the most experience or other logic

            await PostCVThumbnailCard(context, chosen, cv);

            await context.Forward(new ContactDialog(), ResumeAfterDialog, cv, CancellationToken.None);
        }

        private static async Task PostCVThumbnailCard(IDialogContext context, string chosen, CV cv)
        {
            IMessageActivity message = context.MakeMessage();
            ThumbnailCard cvCard = new ThumbnailCard()
            {
                Title = cv.Name,
                Subtitle = cv.Title,
                Text = string.Format(Resources.IndustryChosen2, chosen, cv.Name),
                Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = cv.Image.Small_thumb.Url,
                    }
                }
            };

            Attachment attachment = cvCard.ToAttachment();
            message.Attachments.Add(attachment);

            await context.PostAsync(message);
        }

        private async Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Done(string.Empty);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace SimpleEchoBot.Dialogs
{
    using Models.CVPartner;
    using Properties;
    using Services;
    using SimpleEchoBot.Helpers;

    [Serializable]
    public class IndustryDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait<string>(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var industry = await argument;

            if (!string.IsNullOrEmpty(industry) && industry != "Bransje")
            {
                await context.PostAsync(Resources.IndustryChosen);
                await FindContactForIndustry(context, industry);
            }
            else
            {
                await context.PostAsync(Resources.IndustryChosen);
                context.Wait(PromptChoices);
            }
        }

        private async Task PromptChoices(IDialogContext context, IAwaitable<object> result)
        {
            var configuredIndustries = await IndustryConfiguration.GetConfiguredIndustries();
            PromptDialog.Choice(
                context,
                ProcessChoice,
                configuredIndustries.Industries.Select(ci => ci.Name),
                Resources.IndustryQuestion,
                Resources.SorryChoose,
                3);
        }

        private async Task ProcessChoice(IDialogContext context, IAwaitable<string> result)
        {
            var chosenIndustry = await result;

            //var cvs = await CVPartnerService.Instance.FindCVsForIndustry(chosenIndustry);
            //var cv = cvs.First(); //TODO: Find consultant with the most experience or other logic

            await FindContactForIndustry(context, chosenIndustry);
        }

        private async Task FindContactForIndustry(IDialogContext context, string industry)
        {
            var user = await CVPartnerService.Instance.FindContactForIndustry(industry);

            await PostCVThumbnailCard(context, industry, user);

            await context.Forward(new ContactDialog(), ResumeAfterDialog, user, CancellationToken.None);
        }

        private static async Task PostCVThumbnailCard(IDialogContext context, string chosenIndustry, User user)
        {
            IMessageActivity message = context.MakeMessage();
            ThumbnailCard cvCard = new ThumbnailCard()
            {
                Title = user.Name,
                Subtitle = user.Role.Capitalize().AppendNewline()
                    + user.OfficeName.AppendNewline()
                    + user.Email.AppendNewline()
                    + user.Telephone,
                Text = string.Format(Resources.IndustryChosen2, chosenIndustry, user.Name),
                Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = user.Image.SmallThumb?.Url ?? user.Image.Thumb?.Url ?? user.Image.FitThumb?.Url,
                    }
                },
                //Buttons = new[] { new CardAction { Type = ActionTypes. } }
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
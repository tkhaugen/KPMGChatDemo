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
    public class ServiceDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(PromptChoices);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var service = await argument;

            if (!string.IsNullOrEmpty(service) && service != "Tjeneste")
            {
                await context.PostAsync(Resources.ServiceChosen);
                await FindContactForService(context, service);
            }
            else
            {
                await context.PostAsync(Resources.ServiceChosen);
                context.Wait(PromptChoices);
            }
        }

        private async Task PromptChoices(IDialogContext context, IAwaitable<object> result)
        {
            var configuredServices = await ServiceConfiguration.GetConfiguredServices();
            PromptDialog.Choice(
                context,
                ProcessChoice,
                configuredServices.Services.Select(svc => svc.Name),
                Resources.ServiceQuestion,
                Resources.SorryChoose,
                3);
        }

        private async Task ProcessChoice(IDialogContext context, IAwaitable<string> result)
        {
            var chosenService = await result;

            //var cvs = await CVPartnerService.Instance.FindCVsForIndustry(chosenIndustry);
            //var cv = cvs.First(); //TODO: Find consultant with the most experience or other logic

            await FindContactForService(context, chosenService);
        }

        private async Task FindContactForService(IDialogContext context, string service)
        {
            var user = await CVPartnerService.Instance.FindContactForService(service);

            await PostCVThumbnailCard(context, service, user);

            await context.Forward(new ContactDialog(), ResumeAfterDialog, user, CancellationToken.None);
        }

        private static async Task PostCVThumbnailCard(IDialogContext context, string chosenService, User user)
        {
            IMessageActivity message = context.MakeMessage();
            ThumbnailCard cvCard = new ThumbnailCard()
            {
                Title = user.Name,
                Subtitle = user.Role.Capitalize().AppendNewline()
                    + user.OfficeName.AppendNewline()
                    + user.Email.AppendNewline()
                    + user.Telephone,
                Text = string.Format(Resources.ServiceChosen2, chosenService, user.Name),
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
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using SimpleEchoBot.Properties;
using System.Threading;
using Microsoft.Bot.Connector;

namespace SimpleEchoBot.Dialogs
{
    using Services;
    using Models.CVPartner;
    using SimpleEchoBot.Helpers;
    using System.Collections.Generic;

    [Serializable]
    public class FindContactDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Skriv inn navnet til den ressursen du søker etter.");
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var userName = message.Text;

            var users = await CVPartnerService.Instance.FindContactByName(userName);

            if (users.Count > 0)
            {
                await context.PostAsync("Dette er de ressursene jeg fant:");
                foreach (var user in users)
                    await PostCVThumbnailCard(context, user);

                //await context.Forward(new ContactDialog(), ResumeAfterDialog, user, CancellationToken.None);

                context.Wait(ProcessChoiceAsync);
            }
            else
            {
                await context.PostAsync("Beklager, jeg kunne ikke finne det navnet i vår database.");
                context.Done("");
            }
        }

        private static async Task PostCVThumbnailCard(IDialogContext context, User user)
        {
            IMessageActivity message = context.MakeMessage();
            ThumbnailCard cvCard = new ThumbnailCard()
            {
                Title = user.Name,
                Subtitle = user.Role.Capitalize().AppendNewline()
                    + user.OfficeName.AppendNewline()
                    + user.Email.AppendNewline()
                    + user.Telephone,
                Text = "",
                Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = user.Image.SmallThumb?.Url ?? user.Image.Thumb?.Url ?? user.Image.FitThumb?.Url,
                    }
                },
                Buttons = new[] { new CardAction { Title = "Velg", Text = "Velg", DisplayText = user.Name, Value = user.Email, Type = ActionTypes.PostBack } }
            };

            Attachment attachment = cvCard.ToAttachment();
            message.Attachments.Add(attachment);

            await context.PostAsync(message);
        }

        public async Task ProcessChoiceAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var chosenUser = await argument;

            var user = await CVPartnerService.Instance.GetUser(chosenUser.Text);

            await context.Forward(new ContactDialog(), ResumeAfterDialog, user, CancellationToken.None);
        }

        private async Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Done(string.Empty);
        }
    }
}
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
    public class ResourcesDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument;
            var contactName = activity.Text;

            if (!string.IsNullOrEmpty(contactName))
            {
                //await context.PostAsync(Resources.IndustryChosen);
                await FindContact(context, contactName);
            }
            else
            {
                await context.PostAsync(Resources.PromptResourceName);
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task FindContact(IDialogContext context, string userName)
        {
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
                    + "Kontor: " + user.OfficeName.AppendNewline()
                    + "E-post: " + user.Email.AppendNewline()
                    + "Telefon: " + user.Telephone,
                Text = string.Empty.AppendNewline(),
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
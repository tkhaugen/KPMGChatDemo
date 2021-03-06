﻿using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using SimpleEchoBot.Properties;
using System.Threading;
using SimpleEchoBot.Services;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string _resource = "Ressurs";
        private const string _industry = "Bransje";
        private const string _service = "Tjeneste";

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            PromptQuestion(context);
        }

        private void PromptQuestion(IDialogContext context)
        {
            PromptDialog.Choice(
                context,
                ProcessChoice,
                new string[] { _resource, _industry, _service },
                Resources.InitialQuestion,
                Resources.SorryChoose,
                3);
        }

        private async Task ProcessChoice(IDialogContext context, IAwaitable<string> result)
        {
            var chosen = await result;

            switch (chosen)
            {
                case _resource:
                    context.Call(new ResourcesDialog(), ResumeAfterDialog);
                    break;
                case _industry:
                    await context.Forward(new IndustriesDialog(), ResumeAfterDialog, chosen, CancellationToken.None);
                    break;
                case _service:
                    await context.Forward(new ServicesDialog(), ResumeAfterDialog, chosen, CancellationToken.None);
                    break;
                default:
                    context.Wait(MessageReceivedAsync);
                    break;
            }
        }

        private async Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"Håper du fikk svar på det du lurte på!");
            context.Wait(MessageReceivedAsync);
        }
    }
}
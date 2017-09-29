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

        private Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            Dude(context);
            //PromtQuestion(context);

            return Task.CompletedTask;
        }

        private void Dude(IDialogContext context)
        {
            var cvp = new CVPartnerService();
            var industries = cvp.GetIndustries().GetAwaiter().GetResult();
            foreach (var industry in industries)
                context.PostAsync(industry).GetAwaiter().GetResult();
        }





        private void PromtQuestion(IDialogContext context)
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
                    await context.PostAsync(Resources.NotImplemented);
                    PromtQuestion(context);
                    break;
                case _industry:
                    await context.Forward(new IndustryDialog(), ResumeAfterDialog, chosen, CancellationToken.None);
                    break;
                case _service:
                    await context.PostAsync(Resources.NotImplemented);
                    PromtQuestion(context);
                    break;
                default:
                    context.Wait(MessageReceivedAsync);
                    break;
            }
        }

        private Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.PostAsync($"Håper du fikk svar på det du lurte på!");
            return Task.CompletedTask;
        }
    }
}
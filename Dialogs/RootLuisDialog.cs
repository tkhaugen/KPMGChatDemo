using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using SimpleEchoBot.Properties;

namespace SimpleEchoBot.Dialogs
{
    [LuisModel("ebe0e501-e96c-4328-8fb4-d54fa19530df", "083f39baa8184e0086375810209178d8")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        private const string _resource = "Ressurs";
        private const string _industry = "Bransje";
        private const string _service = "Tjeneste";

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            PromptQuestion(context);
        }

        [LuisIntent("Hils")]
        public async Task Greet(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            PromptQuestion(context);
        }

        [LuisIntent("HentCVForBransje")]
        public async Task Industry(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            EntityRecommendation entityRecommendation;
            if (result.TryFindEntity("Bransje", out entityRecommendation))
            {
                await context.Forward(new IndustryDialog(), ResumeAfterDialog, entityRecommendation.Entity, CancellationToken.None);
            }
            else
            {
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("HentCVForTjenesteomrade")]
        public async Task ServiceArea(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            EntityRecommendation entityRecommendation;
            if (result.TryFindEntity("Tjenesteomrade", out entityRecommendation))
            {
                await context.Forward(new ServiceDialog(), ResumeAfterDialog, entityRecommendation.Entity, CancellationToken.None);
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent("HentCVForRessurs")]
        public async Task Ressurs(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            EntityRecommendation entityRecommendation;
            if (result.TryFindEntity("Ressurs", out entityRecommendation))
            {
                await context.Forward(new FindContactDialog(), ResumeAfterDialog, entityRecommendation.Entity, CancellationToken.None);
            }

            context.Wait(MessageReceived);
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
                    context.Call(new FindContactDialog(), ResumeAfterDialog);
                    break;
                case _industry:
                    await context.Forward(new IndustryDialog(), ResumeAfterDialog, chosen, CancellationToken.None);
                    break;
                case _service:
                    await context.Forward(new ServiceDialog(), ResumeAfterDialog, chosen, CancellationToken.None);
                    break;
                default:
                    context.Wait(MessageReceived);
                    break;
            }
        }

        private async Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"Håper du fikk svar på det du lurte på!");
            context.Wait(MessageReceived);
        }
    }
}
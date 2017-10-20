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
using SimpleEchoBot.Services;

namespace SimpleEchoBot.Dialogs
{
    [LuisModel("ebe0e501-e96c-4328-8fb4-d54fa19530df", "083f39baa8184e0086375810209178d8")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        private const string _resource = "Ressurs";
        private const string _industry = "Bransje";
        private const string _service = "Tjeneste";
        private const string _conv_key_rootChoicesShown = "RootChoicesShown";

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string entity;
            if (SimpleIndustryIntent(result.Query, out entity))
                await ForwardToIndustryDialog(context, entity);
            if (SimpleServiceIntent(result.Query, out entity))
                await ForwardToServiceDialog(context, entity);

            PromptQuestion(context);
        }

        [LuisIntent("Hils")]
        public async Task Greet(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            string entity;
            if (SimpleIndustryIntent(result.Query, out entity))
                await ForwardToIndustryDialog(context, entity);
            if (SimpleServiceIntent(result.Query, out entity))
                await ForwardToServiceDialog(context, entity);

            PromptQuestion(context);
        }

        [LuisIntent("HentCVForBransje")]
        public async Task Industry(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            EntityRecommendation entityRecommendation;
            if (result.TryFindEntity("Bransje", out entityRecommendation))
            {
                await ForwardToIndustryDialog(context, entityRecommendation.Entity);
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
                await ForwardToServiceDialog(context, entityRecommendation.Entity);
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
            string prompt;
            if (context.ConversationData.ContainsKey(_conv_key_rootChoicesShown))
            {
                prompt = Resources.FallbackQuestion;
            }
            else
            {
                context.ConversationData.SetValue(_conv_key_rootChoicesShown, true);
                prompt = Resources.InitialQuestion;
            }

            var options = new PromptOptions<string>(
                prompt,
                Resources.SorryChoose,
                Resources.TooManyAttempts,
                new string[] { _resource, _industry, _service },
                2);

            PromptDialog.Choice(
                context,
                ProcessChoice,
                options);
        }

        private async Task ProcessChoice(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var chosen = await result;

                switch (chosen)
                {
                    case _resource:
                        await ForwardToFindContactDialog(context, string.Empty);
                        break;
                    case _industry:
                        await ForwardToIndustryDialog(context, string.Empty);
                        break;
                    case _service:
                        await ForwardToServiceDialog(context, string.Empty);
                        break;
                    default:
                        context.Wait(MessageReceived);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                context.Done(string.Empty);
            }
        }

        private async Task ForwardToFindContactDialog(IDialogContext context, string contact)
        {
            await context.Forward(new FindContactDialog(), ResumeAfterDialog, contact, CancellationToken.None);
        }

        private async Task ForwardToIndustryDialog(IDialogContext context, string industry)
        {
            await context.Forward(new IndustryDialog(), ResumeAfterDialog, industry, CancellationToken.None);
        }

        private async Task ForwardToServiceDialog(IDialogContext context, string service)
        {
            await context.Forward(new ServiceDialog(), ResumeAfterDialog, service, CancellationToken.None);
        }

        private async Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"Håper du fikk svar på det du lurte på!");
            context.Wait(MessageReceived);
        }

        private bool SimpleIndustryIntent(string message, out string entity)
        {
            entity = null;

            if (string.IsNullOrEmpty(message))
                return false;

            var industries = IndustryConfiguration.GetConfiguredIndustries();
            var industry = industries.Industries.FirstOrDefault(x => x.Variants.Any(v => v.ToLowerInvariant().Contains(message.ToLowerInvariant())));
            if (industry != null)
            {
                entity = industry.Name;
                return true;
            }

            return false;
        }

        private bool SimpleServiceIntent(string message, out string entity)
        {
            entity = null;

            if (string.IsNullOrEmpty(message))
                return false;

            var services = ServiceConfiguration.GetConfiguredServices();
            var service = services.Services.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(message.ToLowerInvariant()));
            if (service != null)
            {
                entity = service.Name;
                return true;
            }

            return false;
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using SimpleEchoBot.Properties;
using System.Threading;
using System.Net.Http;
using Newtonsoft.Json;
using SimpleEchoBot.Models;
using System.Net.Http.Headers;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Bot.Connector;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class IndustryDialog : IDialog<object>
    {
        private static HttpClient Client = new HttpClient();

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(Resources.IndustryChosen);
            context.Wait(PromptChoices);
        }

        private async Task PromptChoices(IDialogContext context, IAwaitable<object> result)
        {
            var industries = await GetIndustries();

            PromptDialog.Choice(
                context,
                this.ChosenAsync,
                industries,
                Resources.IndustryQuestion,
                Resources.SorryChoose,
                3);
        }

        private async Task ChosenAsync(IDialogContext context, IAwaitable<string> result)
        {
            var chosen = await result;
            var cvs = await GetCVs(chosen);
            var cv = cvs.First(); //TODO: Find consultant with the most experience or other logic

            await SendCVThumbnailCard(context, chosen, cv);

            await context.Forward(new ContactDialog(), this.ResumeAfterDialog, cv, CancellationToken.None);
        }

        private static async Task SendCVThumbnailCard(IDialogContext context, string chosen, CV cv)
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

        private static async Task<string[]> GetIndustries()
        {
            try
            {
                var baseUri = ConfigurationManager.AppSettings["CVPartnerUrl"].ToString();

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(baseUri + "/api/v1/unapproved/int/project_experiences/industry?limit=50&offset=0"),
                    Method = HttpMethod.Get,
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Token", ConfigurationManager.AppSettings["CVPartnerKey"].ToString());
                var response = await Client.SendAsync(request);
                if (response != null)
                {
                    var industriesResponse = JsonConvert.DeserializeObject<IndustriesResponse>(await response.Content.ReadAsStringAsync());
                    return industriesResponse.Wrapper.Terms.Select(t => t.Term).ToArray();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static async Task<IEnumerable<CV>> GetCVs(string industry)
        {
            try
            {
                var baseUri = ConfigurationManager.AppSettings["CVPartnerUrl"].ToString();

                var officeIdsString = string.Empty;
                var officeIds = await GetOfficeIds(baseUri);

                foreach (var id in officeIds)
                {
                    officeIdsString += "&office_ids[]=" + id;
                }

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(baseUri + "/api/v3/search?query[0]=" + industry + "&filter_fields[0]=" + officeIdsString + "&size=4&from=0"),
                    Method = HttpMethod.Get,
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Token", ConfigurationManager.AppSettings["CVPartnerKey"].ToString());
                var response = await Client.SendAsync(request);
                if (response != null)
                {
                    var consultantsResponse = JsonConvert.DeserializeObject<ConsultantsResponse>(await response.Content.ReadAsStringAsync());
                    return consultantsResponse.Cvs.Select(c => c.CV);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static async Task<IList<string>> GetOfficeIds(string baseUri)
        {
            try
            {
                var request = new HttpRequestMessage()
                {
                    //TODO: Dont hardcode office_ids or baseurl
                    RequestUri = new Uri(baseUri + "/api/v1/countries"),
                    Method = HttpMethod.Get,
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Token", ConfigurationManager.AppSettings["CVPartnerKey"].ToString());
                var response = await Client.SendAsync(request);
                if (response != null)
                {
                    var countriesResponse = JsonConvert.DeserializeObject<List<CountriesResponse>>(await response.Content.ReadAsStringAsync());

                    var guids = new List<string>();
                    foreach (var country in countriesResponse)
                    {
                        guids.AddRange(country.Offices.Select(o => o.Id));
                    }

                    return guids;

                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
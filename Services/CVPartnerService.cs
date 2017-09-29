using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using SimpleEchoBot.Models;
using System.Net.Http.Headers;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;

namespace SimpleEchoBot.Services
{
    public interface ICVPartnerService
    {
        Task<string[]> GetIndustries();
        Task<IEnumerable<CV>> GetCVs(string industry);
        Task<IList<string>> GetOfficeIds();
    }

    public class CVPartnerService : ICVPartnerService
    {
        private HttpClient _client;
        private string _baseUri;
        private string _authToken;

        public CVPartnerService()
        {
            _client = new HttpClient();
            _baseUri = ConfigurationManager.AppSettings["CVPartnerUrl"];
            _authToken = ConfigurationManager.AppSettings["CVPartnerKey"];
        }

        public async Task<string[]> GetIndustries()
        {
            try
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(_baseUri + "/api/v1/unapproved/int/project_experiences/industry?limit=50&offset=0"),
                    Method = HttpMethod.Get,
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Token", _authToken);
                var response = await _client.SendAsync(request);
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

        public async Task<IEnumerable<CV>> GetCVs(string industry)
        {
            var officeIdsString = string.Empty;
            var officeIds = await GetOfficeIds();

            foreach (var id in officeIds)
            {
                officeIdsString += "&office_ids[]=" + id;
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(_baseUri + "/api/v3/search?query[0]=" + industry + "&filter_fields[0]=" + officeIdsString + "&size=4&from=0"),
                Method = HttpMethod.Get,
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", ConfigurationManager.AppSettings["CVPartnerKey"].ToString());

            var response = await _client.SendAsync(request);

            if (response != null)
            {
                var consultantsResponse = JsonConvert.DeserializeObject<ConsultantsResponse>(await response.Content.ReadAsStringAsync());
                return consultantsResponse.Cvs.Select(c => c.CV);
            }

            return null;
        }

        public async Task<IList<string>> GetOfficeIds()
        {
            var request = new HttpRequestMessage()
            {
                //TODO: Dont hardcode office_ids or baseurl
                RequestUri = new Uri(_baseUri + "/api/v1/countries"),
                Method = HttpMethod.Get,
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", ConfigurationManager.AppSettings["CVPartnerKey"].ToString());

            var response = await _client.SendAsync(request);

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
    }
}
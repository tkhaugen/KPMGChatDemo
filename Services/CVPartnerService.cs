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
            var result = await SendRequest("/api/v1/unapproved/int/project_experiences/industry?limit=50&offset=0");
            var industriesResponse = JsonConvert.DeserializeObject<IndustriesResponse>(result);
            return industriesResponse.Wrapper.Terms.Select(t => t.Term).ToArray();
        }

        public async Task<IEnumerable<CV>> GetCVs(string industry)
        {
            var officeIds = await GetOfficeIds();

            var officeIdsString = string.Empty;
            foreach (var id in officeIds)
            {
                officeIdsString += "&office_ids[]=" + id;
            }
            var path = "/api/v3/search?query[0]=" + industry + "&filter_fields[0]=" + officeIdsString + "&size=4&from=0";

            var result = await SendRequest(path);
            var consultantsResponse = JsonConvert.DeserializeObject<ConsultantsResponse>(result);
            return consultantsResponse.Cvs.Select(c => c.CV);
        }

        public async Task<IList<string>> GetOfficeIds()
        {
            var result = await SendRequest("/api/v1/countries");
            var countriesResponse = JsonConvert.DeserializeObject<List<CountriesResponse>>(result);

            var guids = new List<string>();
            foreach (var country in countriesResponse)
            {
                guids.AddRange(country.Offices.Select(o => o.Id));
            }

            return guids;
        }

        private async Task<string> SendRequest(string path)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(_baseUri + path),
                Method = HttpMethod.Get,
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", _authToken);

            using (var response = await _client.SendAsync(request).ConfigureAwait(false))
            {
                if (response != null)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }

            return null;
        }
    }
}
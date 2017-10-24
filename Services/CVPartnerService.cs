using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;

namespace SimpleEchoBot.Services
{
    using Models.CVPartner;

    public interface ICVPartnerService
    {
        Task<IList<string>> GetIndustries();
        Task<IList<string>> GetSkills();
        Task<IList<CV>> FindCVsForIndustry(string industry);
        Task<IList<CV>> FindCVsForService(string service);
        Task<IList<Office>> GetOffices(string countryCode);
        Task<User> FindContactForIndustry(string industry);
        Task<User> FindContactForService(string service);
        Task<IList<User>> FindContactByName(string name);
        Task<User> GetUser(string email);
    }

    public class CVPartnerService : ICVPartnerService
    {
        private static ICVPartnerService _instance;

        private HttpClient _client;
        private string _baseUri;
        private string _authToken;

        public static ICVPartnerService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CVPartnerService();
                }
                return _instance;
            }
        }

        public CVPartnerService()
        {
            _client = new HttpClient();
            _baseUri = ConfigurationManager.AppSettings["CVPartnerUrl"];
            _authToken = ConfigurationManager.AppSettings["CVPartnerKey"];
        }

        public async Task<IList<string>> GetIndustries()
        {
            var result = await SendRequest("/api/v1/unapproved/int/project_experiences/industry?limit=50&offset=0").ConfigureAwait(false);
            var industriesResponse = JsonConvert.DeserializeObject<IndustriesResponse>(result);
            return industriesResponse.Wrapper.Terms.Select(t => t.Term).ToList();
        }

        public async Task<IList<string>> GetSkills()
        {
            var result = await SendRequest("/api/v1/unapproved/int/technologies/tags?limit=20&offset=0").ConfigureAwait(false);
            var skillsResponse = JsonConvert.DeserializeObject<SkillsResponse>(result);
            return skillsResponse.Wrapper.Terms.Select(t => t.Term).ToList();
        }

        public async Task<IList<CV>> FindCVsForIndustry(string industry)
        {
            // Get all offices in Norway
            var offices = await GetOffices("no");
            
            // Get the industry configuration, which contains all the variants of that industry registered in the CVPartner database
            var industryConfig = IndustryConfiguration.GetConfiguredIndustries().Industries.FirstOrDefault(i => i.Name == industry);
            if (industryConfig == null)
            {
                //throw new Exception($"Unknown industry: {industry}.");
                industryConfig = new Models.Configuration.Industry { Name = industry, Variants = new[] { industry } };
            }
            // Office search string
            var officeIdsString = offices.Aggregate("", (s, o) => s + "&office_ids[]=" + o.Id); // sic!

            var cvs = new Dictionary<string, CV>();
            foreach (var variant in industryConfig.Variants)
            {
                var path = "/api/v3/search?query[0]=" + industry + "&filter_fields[0]=" + officeIdsString + "&size=4&from=0";
                var result = await SendRequest(path);
                var consultantsResponse = JsonConvert.DeserializeObject<ConsultantsResponse>(result);
                foreach (var cvWrapper in consultantsResponse.Cvs)
                {
                    var cv = cvWrapper.CV;
                    if (!cvs.ContainsKey(cv.Name))
                        cvs.Add(cv.Name, cv);
                }
            }

            return cvs.Values.ToList();
        }

        public async Task<User> FindContactForIndustry(string industry)
        {
            var cvs = await FindCVsForIndustry(industry);
            var cv = cvs.FirstOrDefault();
            return await GetUser(cv.Email);
        }

        public async Task<IList<CV>> FindCVsForService(string service)
        {
            // Get all offices in Norway
            var offices = await GetOffices("no");

            // Get the service configuration, which maps offices to services
            var officeForService = ServicesConfiguration.GetConfiguredServices().Services.FirstOrDefault(s => s.Name.Equals(service, StringComparison.OrdinalIgnoreCase));
            if (officeForService == null)
                return new CV[0];

            var office = offices.FirstOrDefault(o => o.Name == officeForService.Office);
            if (office == null)
                throw new Exception($"Office '{officeForService.Name}' not found in CVPartner.");

            // Office search string
            var officeIdsString = "&office_ids[]=" + office.Id;
            var serviceRole = ServicesConfiguration.GetPreferredRoles().First();

            var cvs = new Dictionary<string, CV>();
            var path = "/api/v3/search?query[0]=" + serviceRole + "&filter_fields[0]=" + officeIdsString + "&size=4&from=0";
            var result = await SendRequest(path);
            var consultantsResponse = JsonConvert.DeserializeObject<ConsultantsResponse>(result);
            foreach (var cvWrapper in consultantsResponse.Cvs)
            {
                var cv = cvWrapper.CV;
                if (!cvs.ContainsKey(cv.Name))
                    cvs.Add(cv.Name, cv);
            }

            return cvs.Values.ToList();
        }

        public async Task<User> FindContactForService(string service)
        {
            var cvs = await FindCVsForService(service);
            var cv = cvs.FirstOrDefault();
            return await GetUser(cv.Email);
        }

        public async Task<IList<CV>> FindCVsForName(string industry)
        {
            // Get all offices in Norway
            var offices = await GetOffices("no");

            // Office search string
            var officeIdsString = offices.Aggregate("", (s, o) => s + "&office_ids[]=" + o.Id); // sic!

            var cvs = new List<CV>();
            var path = "/api/v3/search?query[0]=" + industry + "&filter_fields[0]=" + officeIdsString + "&size=4&from=0";
            var result = await SendRequest(path);
            var consultantsResponse = JsonConvert.DeserializeObject<ConsultantsResponse>(result);
            foreach (var cvWrapper in consultantsResponse.Cvs)
            {
                var cv = cvWrapper.CV;
                cvs.Add(cv);
            }

            return cvs.ToList();
        }

        public async Task<IList<User>> FindContactByName(string name)
        {
            var cvs = await FindCVsForName(name);
            var users = new List<User>();

            foreach(var cv in cvs.Where(x => !string.IsNullOrEmpty(x.Email)))
            {
                var user = await GetUser(cv.Email);
                if (user != null)
                {
                    users.Add(user);
                }
            }

            return users;
        }

        public async Task<User> GetUser(string email)
        {
            var result = await SendRequest("/api/v1/users/find?email=" + email);
            if (result == null)
                return null;

            try
            {
                return JsonConvert.DeserializeObject<User>(result);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IList<Office>> GetOffices(string countryCode)
        {
            var result = await SendRequest("/api/v1/countries");
            var countriesResponse = JsonConvert.DeserializeObject<List<CountriesResponse>>(result);

            var country = countriesResponse.SingleOrDefault(c => c.Code == countryCode);

            return country?.Offices ?? new List<Office>();
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
                if (response == null || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
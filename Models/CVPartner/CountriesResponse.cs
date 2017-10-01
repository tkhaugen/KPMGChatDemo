using Newtonsoft.Json;
using System.Collections.Generic;

namespace SimpleEchoBot.Models.CVPartner
{
    public class CountriesResponse
    {
        public string Code { get; set; }

        public List<Office> Offices { get; set; }
    }

    public class Office
    {
        [JsonProperty(PropertyName = "_id")]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
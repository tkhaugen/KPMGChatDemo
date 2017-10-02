using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace SimpleEchoBot.Models.CVPartner
{
    public class SkillsResponse
    {
        public SkillsWrapper Wrapper { get; set; }
    }

    public class SkillsWrapper
    {
        public List<Skill> Terms { get; set; }
    }

    public class Skill
    {
        public string Term { get; set; }
        public int Count { get; set; }
        [JsonProperty("matching_categories")]
        public List<string> MatchingCategories { get; set; }
    }

}
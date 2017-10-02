using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace SimpleEchoBot.Models.CVPartner
{
    public class User
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        //public string _id { get; set; }
        public string Id { get; set; }
        [JsonProperty("company_id")]
        public string CompanyId { get; set; }
        [JsonProperty("company_name")]
        public string CompanyName { get; set; }
        [JsonProperty("company_subdomains")]
        public string[] CompanySubdomains { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("external_unique_id")]
        public string ExternalUniqueId { get; set; }
        [JsonProperty("deactivated")]
        public bool Deactivated { get; set; }
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("roles")]
        public string[] Roles { get; set; }
        [JsonProperty("office_id")]
        public string OfficeId { get; set; }
        [JsonProperty("office_name")]
        public string OfficeName { get; set; }
        [JsonProperty("country_id")]
        public string CountryId { get; set; }
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        [JsonProperty("language_code")]
        public string LanguageCode { get; set; }
        [JsonProperty("language_codes")]
        public string[] LanguageCodes { get; set; }
        [JsonProperty("international_toggle")]
        public string InternationalToggle { get; set; }
        [JsonProperty("preferred_download_format")]
        public string PreferredDownloadFormat { get; set; }
        [JsonProperty("masterdata_languages")]
        public string[] MasterdataLanguages { get; set; }
        [JsonProperty("expand_proposals_toggle")]
        public bool ExpandProposalsToggle { get; set; }
        [JsonProperty("selected_office_ids")]
        public object[] SelectedOfficeIds { get; set; }
        [JsonProperty("override_language_code")]
        public object OverrideLanguageCode { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("telephone")]
        public string Telephone { get; set; }
        [JsonProperty("default_cv_id")]
        public string DefaultCvId { get; set; }
        [JsonProperty("image")]
        public Image Image { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace SimpleEchoBot.Services
{
    using System.Configuration;
    using Models.Configuration;

    public class IndustryConfiguration
    {
        private static ConfiguredIndustries _configuredIndustries;

        public static ConfiguredIndustries GetConfiguredIndustries()
        {
            if (_configuredIndustries == null)
            {
                var json = ReadFile("~/Config/Industries.json");
                _configuredIndustries = JsonConvert.DeserializeObject<ConfiguredIndustries>(json);
            }

            return _configuredIndustries;
        }

        public static string[] GetPreferredRoles()
        {
            return ConfigurationManager.AppSettings["Industries_PreferredRoles"].Split(',');
        }

        private static string ReadFile(string virtualPath)
        {
            var path = HostingEnvironment.MapPath(virtualPath);
            string content;
            using (var reader = new StreamReader(path))
            {
                content = reader.ReadToEnd();
            }
            return content;
        }
    }
}
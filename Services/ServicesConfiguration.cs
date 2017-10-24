using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace SimpleEchoBot.Services
{
    using Models.Configuration;

    public class ServicesConfiguration
    {
        private static ConfiguredServices _configuredServices;

        public static ConfiguredServices GetConfiguredServices()
        {
            if (_configuredServices == null)
            {
                var json = ReadFile("~/Config/Services.json");
                _configuredServices = JsonConvert.DeserializeObject<ConfiguredServices>(json);
            }

            return _configuredServices;
        }

        public static string[] GetPreferredRoles()
        {
            return ConfigurationManager.AppSettings["Services_PreferredRoles"].Split(',');
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
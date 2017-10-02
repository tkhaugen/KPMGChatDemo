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

    public class ServiceConfiguration
    {
        private static ConfiguredServices _configuredServices;

        public static async Task<ConfiguredServices> GetConfiguredServices()
        {
            if (_configuredServices != null)
                return _configuredServices;

            var json = await ReadFileAsync("~/Config/Services.json");
            var configuredServices = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ConfiguredServices>(json));
            return configuredServices;
        }

        public static string[] GetPreferredRoles()
        {
            return ConfigurationManager.AppSettings["Service_PreferredRoles"].Split(',');
        }

        private static async Task<string> ReadFileAsync(string virtualPath)
        {
            var path = HostingEnvironment.MapPath(virtualPath);
            string content;
            using (var reader = new StreamReader(path))
            {
                content = await reader.ReadToEndAsync();
            }
            return content;
        }
    }
}
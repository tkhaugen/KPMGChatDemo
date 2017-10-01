using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Services
{
    using System.IO;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using Models.Configuration;
    using Newtonsoft.Json;

    public class IndustryConfiguration
    {
        private static ConfiguredIndustries _configuredIndustries;

        public static async Task<ConfiguredIndustries> GetConfiguredIndustries()
        {
            if (_configuredIndustries != null)
                return _configuredIndustries;

            var json = await ReadFileAsync("~/Config/Industries.json");
            var configuredIndustries = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ConfiguredIndustries>(json));
            return configuredIndustries;
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
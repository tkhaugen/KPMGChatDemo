﻿using System;
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

        public static async Task<ConfiguredIndustries> GetConfiguredIndustries()
        {
            if (_configuredIndustries != null)
                return _configuredIndustries;

            var json = await ReadFileAsync("~/Config/Industries.json");
            var configuredIndustries = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ConfiguredIndustries>(json));
            return configuredIndustries;
        }

        public static string[] GetPreferredRoles()
        {
            return ConfigurationManager.AppSettings["Industry_PreferredRoles"].Split(',');
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
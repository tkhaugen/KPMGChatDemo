using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Models.Configuration
{
    public class ConfiguredServices
    {
        public Service[] Services { get; set; }
    }

    public class Service
    {
        public string Name { get; set; }
        public string Office { get; set; }
    }
}
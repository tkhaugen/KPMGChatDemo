using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace SimpleEchoBot.Services
{
    public class MailConfiguration
    {
        public static string Sender => ConfigurationManager.AppSettings["Mail_Sender"];
        public static string[] Recipients => ConfigurationManager.AppSettings["Mail_Recipients"].Split(';').Select(r => r.Trim()).ToArray();
        public static string SendGridApiKey => ConfigurationManager.AppSettings["Mail_SendGrid_ApiKey"];
        public static string SendGridUserName => ConfigurationManager.AppSettings["Mail_SendGrid_UserName"];
        public static string SendGridPassword => ConfigurationManager.AppSettings["Mail_SendGrid_Password"];
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Models.CVPartner
{
    public class IndustriesResponse
    {
        public IndustriesWrapper Wrapper { get; set; }
    }

    public class IndustriesWrapper
    {
        public List<Industry> Terms { get; set; }
    }

    public class Industry
    {
        public string Term { get; set; }
        public int Count { get; set; }
    }
}
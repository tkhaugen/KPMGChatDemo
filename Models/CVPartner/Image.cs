using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Models.CVPartner
{
    public class Image
    {
        public string Url { get; set; }
        public ImageUrl Thumb { get; set; }
        public ImageUrl FitThumb { get; set; }
        public ImageUrl Large { get; set; }
        public ImageUrl SmallThumb { get; set; }
    }

    public class ImageUrl
    {
        public string Url { get; set; }
    }
}
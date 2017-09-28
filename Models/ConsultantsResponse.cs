using System.Collections.Generic;

namespace SimpleEchoBot.Models
{
    public class ConsultantsResponse
    {
        public List<CVWrapper> Cvs { get; set; }
    }

    public class CVWrapper
    {
        public CV CV { get; set; }
    }
    public class CV
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public Image Image { get; set; }
    }

    public class Image
    {
        public ImageUrl Thumb { get; set; }
        public ImageUrl Small_thumb { get; set; }
    }

    public class ImageUrl
    {
        public string Url { get; set; }
    }
}
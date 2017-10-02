using System.Collections.Generic;

namespace SimpleEchoBot.Models.CVPartner
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
}
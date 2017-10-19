using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;

namespace SimpleEchoBot.Models
{
    [Serializable]
    public class ContactDetails
    {
        [Prompt("Navn")]
        public string Name { get; set; }

        [Prompt("E-postadresse")]
        public string Email { get; set; }

        [Prompt("Telefonnummer")]
        public string Phone { get; set; }

        public static IForm<ContactDetails> BuildForm()
        {
            return new FormBuilder<ContactDetails>()
                .Message("Vennligst skriv inn kontaktinformasjon.")
                .AddRemainingFields()
                //.OnCompletion(async (context, form) =>
                //{
                //    // Tell the user that the form is complete  
                //    await context.PostAsync("Er dette riktig?");
                //})
                .Build();
        }
    }
}
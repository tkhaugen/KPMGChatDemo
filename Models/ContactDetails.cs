using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System.Text.RegularExpressions;
using SimpleEchoBot.Helpers;
using SimpleEchoBot.Properties;

namespace SimpleEchoBot.Models
{
    [Serializable]
    public class ContactDetails
    {
        private const string _noValue = "(ingen)";

        [Prompt("Navnet ditt:")]
        public string Name { get; set; }

        [Prompt("E-postadresse:")]
        public string Email { get; set; }

        [Prompt("Telefonnummer:")]
        public string Phone { get; set; }

        public static IForm<ContactDetails> BuildForm()
        {
            return new FormBuilder<ContactDetails>()
                .Message("Vennligst skriv inn kontaktinformasjon som vi kan nå deg med:")
                .Field(nameof(Name))
                .Field(nameof(Email),
                    validate: async (state, value) =>
                    {
                        var email = (string)value;
                        if (IsSkipCommand(email))
                            return new ValidateResult { IsValid = true, Value = _noValue };
                        if (ValidateEmailAddress(email))
                            return new ValidateResult { IsValid = true, Value = email };
                        return new ValidateResult { IsValid = false, Feedback = string.Format(Resources.InvalidEmail, email), Value = "" };
                    })
                .Field(nameof(Phone),
                    validate: async (state, value) =>
                    {
                        var phone = (string)value;
                        if (IsSkipCommand(phone))
                            return new ValidateResult { IsValid = true, Value = _noValue };
                        if (ValidatePhoneNumber(phone))
                            return new ValidateResult { IsValid = true, Value = phone };
                        return new ValidateResult { IsValid = false, Feedback = string.Format(Resources.InvalidPhone, phone), Value = "" };
                    })
                //.OnCompletion(async (context, form) =>
                //{
                //    // Tell the user that the form is complete  
                //    await context.PostAsync("Er dette riktige opplysninger?".AppendNewline()
                //        + "Navn: " + form.Name);
                //})
                .Build();
        }

        private static bool IsSkipCommand(string input)
        {
            if (input == null)
                return false;

            return input.Trim().ToLower() == "ingen";
        }

        private static bool ValidateEmailAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;
            try
            {
                new System.Net.Mail.MailAddress(address);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool ValidatePhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;
            var stripped = phone.TrimStart('+');
            return Regex.Match(stripped, @"\d{2,}").Success;
        }
    }
}
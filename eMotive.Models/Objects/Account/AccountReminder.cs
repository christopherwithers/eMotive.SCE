
using eMotive.Models.Validation.Account;
using ServiceStack.FluentValidation.Attributes;


namespace eMotive.Models.Objects.Account
{
    [Validator(typeof(AccountReminderValidator))]
    public class AccountReminder
    {
        public string EmailAddress { get; set; }
    }
}

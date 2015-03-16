using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.Account
{
    public class AccountReminderValidator : AbstractValidator<Objects.Account.AccountReminder>
    {
        public AccountReminderValidator()
        {
            RuleFor(n => n.EmailAddress).NotEmpty().WithMessage("Please enter your email address.").EmailAddress().WithMessage("Please enter a valid email address.");
        }
    }
}

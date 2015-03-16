using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.Account
{
    public class LoginValidator : AbstractValidator<Objects.Account.Login>
    {
        public LoginValidator()
        {
            RuleFor(n => n.UserName).NotEmpty().WithMessage("Please enter your username.");
            RuleFor(n => n.Password).NotEmpty().WithMessage("Please enter your password.");
        }
    }
}

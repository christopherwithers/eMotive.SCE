


using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.User
{
    public class UserValidator : AbstractValidator<Objects.Users.User>
    {
        public UserValidator()
        {
            RuleFor(n => n.Username).NotEmpty().WithMessage("Please specify a username");
            RuleFor(n => n.Forename).NotEmpty().WithMessage("Please specify a forename");
            RuleFor(n => n.Surname).NotEmpty().WithMessage("Please specify a surname");
            RuleFor(n => n.Email).EmailAddress().NotEmpty().WithMessage("Please specify an email address");
            RuleFor(n => n.Roles).NotEmpty().WithMessage("Please select one or more roles");
        }
    }
}

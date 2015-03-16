using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.User
{
    public class UserWithRolesValidator : AbstractValidator<Objects.Users.UserWithRoles>
    {
        public UserWithRolesValidator()
        {
            RuleFor(n => n.Username).NotEmpty().WithMessage("Please specify a username");
            RuleFor(n => n.Forename).NotEmpty().WithMessage("Please specify a forename");
            RuleFor(n => n.Surname).NotEmpty().WithMessage("Please specify a surname");
            RuleFor(n => n.Email).EmailAddress().NotEmpty().WithMessage("Please specify an email address");
            RuleFor(n => n.SelectedRole).NotEmpty().WithMessage("Please select a role");
        }
    }
}

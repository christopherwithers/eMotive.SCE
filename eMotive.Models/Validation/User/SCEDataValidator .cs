using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.User
{
    public class SCEDataValidator : AbstractValidator<Objects.Users.SCEData>
    {
        public SCEDataValidator()
        {
            RuleFor(n => n.Username).NotEmpty().WithMessage("Please specify a username");
            RuleFor(n => n.Forename).NotEmpty().WithMessage("Please specify a forename");
            RuleFor(n => n.Surname).NotEmpty().WithMessage("Please specify a surname");
            RuleFor(n => n.Email).EmailAddress().NotEmpty().WithMessage("Please specify an email address");
            RuleFor(n => n.BelongsToGroups).NotEmpty().WithMessage("Please select one or more groups");
            //  RuleFor(n => n.Roles).NotEmpty().WithMessage("Please select one or more roles");
        }
    }
}

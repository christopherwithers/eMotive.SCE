using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.Sessions
{
    public class GroupValidator : AbstractValidator<Objects.Signups.Group>
    {
        public GroupValidator()
        {
            RuleFor(n => n.Name).NotEmpty().WithMessage("Please specify a name.");
        }
    }
}

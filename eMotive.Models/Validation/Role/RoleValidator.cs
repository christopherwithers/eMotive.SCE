using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.Role
{
    public class RoleValidator : AbstractValidator<Objects.Roles.Role>
    {
        public RoleValidator()
        {
            RuleFor(n => n.Name).NotEmpty().WithMessage("Please specify a role name");
            RuleFor(n => n.Colour).NotEmpty().WithMessage("Please specify a colour");
            RuleFor(n => n.Name).Length(3, 20).WithMessage("The role name should be between 3 and 20 chracters long");
        }
    }
}

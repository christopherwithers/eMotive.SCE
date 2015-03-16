using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.Page
{
    public class PartialPageValidator : AbstractValidator<Objects.Pages.PartialPage>
    {
        public PartialPageValidator()
        {
            RuleFor(n => n.Key).NotEmpty().WithMessage("The key should not be empty.").Length(5, 40).WithMessage("The key should be between 5 and 40 characters long.");
            RuleFor(n => n.Text).NotEmpty().WithMessage("The page body should not be empty.");
        }
    }
}

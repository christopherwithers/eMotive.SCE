using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.Page
{
    public class PageValidator : AbstractValidator<Objects.Pages.Page>
    {
        public PageValidator()
        {
            RuleFor(n => n.Title).NotEmpty().WithMessage("The page title should not be empty.").Length(5, 60).WithMessage("The page title should be between 5 and 60 chracters long.");
            RuleFor(n => n.Body).NotEmpty().WithMessage("The page body should not be empty.");
        }
    }
}

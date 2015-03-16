using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.News
{
    public class NewsValidator : AbstractValidator<Objects.News.NewsItem>
    {
        public NewsValidator()
        {
            RuleFor(n => n.Title).NotEmpty().WithMessage("The News item title should not be empty.").Length(5, 100).WithMessage("The News item title should be between 5 and 100 chracters long.");
            RuleFor(n => n.Body).NotEmpty().WithMessage("The News Item body should not be empty.");
        }
    }
}

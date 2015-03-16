using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.Email
{
    public class EmailValidator : AbstractValidator<Objects.Email.Email>
    {
        public EmailValidator()
        {
            RuleFor(n => n.Key).NotEmpty().WithMessage("The key should not be empty.").Length(5, 30).WithMessage("The key should be between 5 and 30 characters long.");
            RuleFor(n => n.Title).NotEmpty().WithMessage("The email title should not be empty.").Length(5, 100).WithMessage("The email title should be between 5 and 100 chracters long.");
            RuleFor(n => n.Message).NotEmpty().WithMessage("The email body should not be empty.");
        }
    }
}

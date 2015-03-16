using ServiceStack.FluentValidation;

namespace eMotive.Services.Objects.Validation
{
    public class SettingsValidator : AbstractValidator<Settings.Settings>
    {
        public SettingsValidator()
        {
            RuleFor(n => n.SiteName).NotEmpty().WithMessage("Please enter the site name.");
            RuleFor(n => n.SiteURL).NotEmpty().WithMessage("Please enter the site URL.");
            RuleFor(n => n.MailFromAddress).NotEmpty().WithMessage("Please enter the 'mail from' address.");
            RuleFor(n => n.MaxLoginAttempts).NotNull().WithMessage("Please enter the max number of login attempts.").GreaterThan(0);
            RuleFor(n => n.LockoutTimeMinutes).NotNull().WithMessage("Please enter the lockout time limit.").GreaterThan(0);
            //RuleFor(n => n.MetaTags).NotEmpty().WithMessage("Please enter the site URL.");
            //RuleFor(n => n.SiteURL).NotEmpty().WithMessage("Please enter the site URL.");
         //   RuleFor(n => n.Password).NotEmpty().WithMessage("Please enter your password.");
        }
    }
}

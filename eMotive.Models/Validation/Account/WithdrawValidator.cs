using ServiceStack.FluentValidation;

namespace eMotive.Models.Validation.Account
{
    public class WithdrawValidator : AbstractValidator<Objects.Account.Withdraw>
    {
        public WithdrawValidator()
        {
            RuleFor(n => n.WithdrawalConfirmation).NotEmpty().WithMessage("If you wish to withdraw, please check the box.");
        }
    }
}

using eMotive.Models.Validation.Account;
using ServiceStack.FluentValidation.Attributes;

namespace eMotive.Models.Objects.Account
{
    [Validator(typeof(WithdrawValidator))]
    public class Withdraw
    {
        public string PageText { get; set; }
        public bool WithdrawalConfirmation { get; set; }
    }
}

using ServiceStack.FluentValidation.Attributes;
using eMotive.Models.Validation.Account;

namespace eMotive.Models.Objects.Account
{
    [Validator(typeof(ChangePasswordValidator))]
    public class ChangePassword
    {
        public string Username { get; set; }
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
        public string NewPasswordRetype { get; set; }
    }
}

using ServiceStack.FluentValidation.Attributes;
using eMotive.Models.Validation.Account;

namespace eMotive.Models.Objects.Account
{
    [Validator(typeof(LoginValidator))]
    public class Login
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}

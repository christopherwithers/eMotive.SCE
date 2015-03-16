using eMotive.Models.Objects.Users;

namespace eMotive.Models.Objects.Signups
{
    public class ThirdPartySignupSessions
    {
        public User User { get; set; }
        public UserSignupView Signup { get; set; }
    }
}

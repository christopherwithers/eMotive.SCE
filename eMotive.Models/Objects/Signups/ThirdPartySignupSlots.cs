using eMotive.Models.Objects.Users;

namespace eMotive.Models.Objects.Signups
{
    public class ThirdPartySignupSlots
    {
        public User User { get; set; }
        public UserSlotView Slots { get; set; }
    }
}

using System;
using eMotive.Models.Objects.Users;

namespace eMotive.Models.Objects.Signups
{
    public class UserSignup
    {
        public int ID { get; set; }
        public User User { get; set; }
        public DateTime SignupDate { get; set; }
   //     public SlotType Type { get; set; }
    }
}

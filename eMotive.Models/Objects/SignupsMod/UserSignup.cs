using System;
using eMotive.Models.Objects.Users;

namespace eMotive.Models.Objects.SignupsMod
{
    public class UserSignup
    {
        public int ID { get; set; }
        public int IdSlot { get; set; }
        public int IdUser { get; set; }
        public User User { get; set; }
        public int IdSignUp { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public SlotType Type { get; set; }
        public DateTime SignUpDate { get; set; }
    }
}

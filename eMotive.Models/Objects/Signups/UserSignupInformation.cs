using System;

namespace eMotive.Models.Objects.Signups
{
    public class UserSignupInformation
    {
        public int SignupID { get; set; }
        public int SlotID { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}

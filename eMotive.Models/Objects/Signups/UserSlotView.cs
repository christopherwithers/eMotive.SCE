using System.Collections.Generic;
using eMotive.Models.Objects.Users;

namespace eMotive.Models.Objects.Signups
{
    public class UserSlotView : Signup
    {
        public UserSlotView(Signup _signup)
        {
            ID = _signup.ID;
            Date = _signup.Date;
            CloseDate = _signup.CloseDate;
            Group = _signup.Group;
            AcademicYear = _signup.AcademicYear;
            Closed = _signup.Closed;
            OverrideClose = _signup.OverrideClose;
            MergeReserve = _signup.MergeReserve;
            AllowMultipleSignups = _signup.AllowMultipleSignups;

            Slots = _signup.Slots;



        }

        public bool HasSignedUp { get; set; }

        public string SignupDescription { get; set; }

        public IEnumerable<SlotState> SlotState { get; set; }

        public string HeaderText { get; set; }
        public string FooterText { get; set; }

        public string LoggedInUser { get; set; }
    }
}

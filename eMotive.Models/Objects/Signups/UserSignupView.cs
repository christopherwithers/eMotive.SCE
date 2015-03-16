using System.Collections.Generic;
using System.Linq;
using eMotive.Models.Objects.Users;
using Extensions;
using ServiceStack.Common;

namespace eMotive.Models.Objects.Signups
{
    public class UserSignupView
    {
        public string HeaderText { get; set; }
        public string FooterText { get; set; }
        public bool SignedUp { get; set; }
        public int SignupID { get; set; }
        public User LoggedInUser { get; set; }

        public IEnumerable<WillingToChangeSignup> WillingToChange { get; set; }

        private IDictionary<int, bool> WillingToChangeDict; 

        public ICollection<SignupState> SignupInformation { get; set; }
        public IDictionary<string, Group> GroupDictionary { get; set; }

        public bool WillingToChangeCheck(int signupID)
        {
            if (WillingToChangeDict.IsEmpty() && WillingToChange.HasContent())
            {
                WillingToChangeDict = WillingToChange.ToDictionary(k => k.SignupID, v => v.UserID == LoggedInUser.ID);
            }

            if (WillingToChangeDict.HasContent())
            {
                bool contains;

                WillingToChangeDict.TryGetValue(signupID, out contains);

                return contains;
            }

            return false;
        }

        public IEnumerable<KeyValuePair<string, List<SignupState>>> GetSignupsByGroup()
        {
            if (!SignupInformation.HasContent())
                return null;

            return SignupInformation.GroupBy(m => m.Group.Name).ToDictionary(k => k.Key, v => v.ToList());
        }
    }
}

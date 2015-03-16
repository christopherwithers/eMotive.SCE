using System.Collections.Generic;
using System.Linq;
using eMotive.Models.Objects.Users;
using Extensions;

namespace eMotive.Models.Objects.Signups
{
    public class AdminSignupView
    {
        public IEnumerable<SignupsMod.Signup> Signups { get; set; }
        public User LoggedInUser { get; set; }
        public IDictionary<string, List<SignupsMod.Signup>> GetSignupsGroupedByGroup()
        {
            if (!Signups.HasContent())
                return null;

            var dict = Signups.GroupBy(m => m.Group.Name).ToDictionary(k => k.Key, v => v.ToList());

            return dict;
        }


        public IDictionary<string, List<SignupsMod.Signup>> GetSignupsGroupedByLocation()
        {
            if (!Signups.HasContent())
                return null;

            var dict = Signups.GroupBy(m => m.Location.Name).ToDictionary(k => k.Key, v => v.ToList());

            return dict;
        }
    }
}

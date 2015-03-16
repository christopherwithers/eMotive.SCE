using System.Collections.Generic;
using System.Collections.ObjectModel;
using eMotive.Models.Objects.Search;

namespace eMotive.Models.Objects.SignupsMod
{
    public class SignupSearch : BasicSearch
    {
        public SignupSearch()
        {
            ItemType = "Signups";

          //  RoleFilter =  //new[] {"All", "Admin", "Interviewer", "Applicant"};
        }

        public IEnumerable<Signup> Signups { get; set; }

        public Collection<KeyValuePair<string, string>> GroupFilter { get; set; }
        public string SelectedGroupFilter { get; set; }

        public Collection<KeyValuePair<string, string>> LocationFilter { get; set; }
        public string SelectedLocationFilter { get; set; }

       /* public IDictionary<string, List<Signup>> GetSignupsGroupedByGroup()
        {
            if (!Signups.HasContent())
                return null;

            var dict = Signups.GroupBy(m => m.Group.Name).ToDictionary(k => k.Key, v => v.ToList());

            return dict;
        }*/
    }
}

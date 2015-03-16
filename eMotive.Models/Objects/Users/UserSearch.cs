using System.Collections.Generic;
using System.Collections.ObjectModel;
using eMotive.Models.Objects.Search;

namespace eMotive.Models.Objects.Users
{
    public class UserSearch : BasicSearch
    {
        public UserSearch()
        {
            ItemType = "User";

          //  RoleFilter =  //new[] {"All", "Admin", "Interviewer", "Applicant"};
        }

        public IEnumerable<User> Users { get; set; }

        public Collection<KeyValuePair<string, string>> RoleFilter { get; set; }
        public string SelectedRoleFilter { get; set; }
       // public bool CanCreate { get; set; }
    }
}

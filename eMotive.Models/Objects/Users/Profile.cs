using System.Collections.Generic;
using eMotive.Models.Objects.Signups;

namespace eMotive.Models.Objects.Users
{
    public class Profile
    {
        public IEnumerable<Group> Groups { get; set; }
    }
}

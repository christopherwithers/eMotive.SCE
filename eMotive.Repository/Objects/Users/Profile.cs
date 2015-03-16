using System.Collections.Generic;
using eMotive.Repository.Objects.Signups;

namespace eMotive.Repository.Objects.Users
{
    public class Profile
    {
        public IEnumerable<Group> Groups { get; set; }
        public IEnumerable<ApplicantData> ApplicantFields { get; set; } 
    }
}

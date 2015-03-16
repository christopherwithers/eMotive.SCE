using System;
using System.Collections.Generic;
using eMotive.Repository.Objects.Signups;
using Extensions;

namespace eMotive.Managers.Objects.Signups
{
    /// <summary>
    /// Used SignupManager.GenerateSlotStatus to forulate slot statuses based upon current user signups
    /// </summary>
    public class GenerateSlotStatusDTO
    {
       /* public GenerateSlotStatusDTO()
        {
            MultipleSignupsPerGroup = new Dictionary<Group, bool>();
        }*/
        public int SignupID { get; set; }
        public DateTime SignupDate { get; set; }
        public bool MergeReserve { get; set; }
        public string Username { get; set; }
        public bool MultipleSignupsPerSignup { get; set; }
       public bool MultipleSignupsPerGroup { get; set; }
    //   public IDictionary<Group, bool> MultipleSignupsPerGroup { get; set; } 
        public string Group { get; set; }
        public bool Closed { get; set; }
        public bool OverrideClose { get; set; }

        public bool UserHasSignup { get { return UsersSignups.HasContent(); }}
        public IEnumerable<UserSignup> UsersSignups { get; set; }
    }
}

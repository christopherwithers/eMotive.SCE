using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using eMotive.Models.Objects.Pages;
using eMotive.Models.Objects.Users;
using Extensions;

namespace eMotive.Models.Objects.Signups
{
    public class UserHomeView
    {
        public UserHomeView()
        {
            PageSections = new Dictionary<string, PartialPage>();
            SignupDetails = new Collection<UserSignupDetails>();

            ButtonText = "You have not yet signed up for any sessions. Click here to do so.";
        }

        public User User { get; set; }
        public bool HasSignedUp { get { return SignupDetails.HasContent(); } }

        public ICollection<UserSignupDetails> SignupDetails { get; set; } 

        public IDictionary<string, PartialPage> PageSections { get; set; }

        public string ButtonText { get; set; }
    }

    //TODO: put this in its own .cs file
    public class UserSignupDetails
    {
        public DateTime SignUpDate { get; set; }
        public string SignUpDetails { get; set; }
        public int SignedUpSlotID { get; set; }
        public int SignupID { get; set; }
        public Group SignupGroup { get; set; }
        public string SignupDescription { get; set; }
        public SlotType Type { get; set; }
        public Location Location { get; set; }

    }
}

using System;
using System.Collections.Generic;

namespace eMotive.Models.Objects.Signups
{
    public class Slot
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public int TotalPlacesAvailable { get; set; }
        public int ReservePlaces { get; set; }
        public int InterestedPlaces { get; set; }
        public bool Enabled { get; set; }
        public DateTime Time { get; set; }
        public ICollection<UserSignup> ApplicantsSignedUp { get; set; }

    }
}

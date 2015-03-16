using System;
using System.Collections.Generic;

namespace eMotive.Models.Objects.Signups
{
    public class Signup
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public DateTime CloseDate { get; set; }
        public Group Group { get; set; }
        public Location Location { get; set; }
        public string AcademicYear { get; set; }
        public bool Closed { get; set; }
        public bool OverrideClose { get; set; }
        public bool MergeReserve { get; set; }
        public bool AllowMultipleSignups { get; set; }

        public bool PlacementTags { get; set; }
        public bool DetailedPlacementText { get; set; }

        public string Description { get; set; }

        public ICollection<Slot> Slots { get; set; }
    }
}

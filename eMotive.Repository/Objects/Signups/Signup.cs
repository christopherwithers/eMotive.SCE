using System;
using System.Collections.Generic;

namespace eMotive.Repository.Objects.Signups
{
    public class Signup
    {
        public int id { get; set; }
        public DateTime Date { get; set; }
        public DateTime CloseDate { get; set; }
        public Group Group { get; set; }
        public int idSite { get; set; }
        public string AcademicYear { get; set; }
        public bool Closed { get; set; }
        public bool OverrideClose { get; set; }
        public bool MergeReserve { get; set; }
        public bool AllowMultipleSignups { get; set; }
        public bool IsTraining { get; set; }

        public bool PlacementTags { get; set; }
        public bool DetailedPlacementText { get; set; }

        public int IdGroup { get; set; }

        public string Description { get; set; }

        public IEnumerable<Slot> Slots { get; set; }
    }
}

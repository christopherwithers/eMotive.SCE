using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace eMotive.Models.Objects.Signups
{
    public class SignupState
    {
        public SignupState()
        {
            SignupNumbers = new Collection<SignupSlotState>();
        }

        public int ID { get; set; }
        public DateTime Date { get; set; }
        //public int SlotsAvailable { get; set; }
        public bool SignedUp { get; set; }
        public int NumberSignedUp { get; set; }
        
        public int TotalSlotsAvailable { get; set; }
        public int TotalReserveAvailable { get; set; }
        public int TotalInterestedAvaiable { get; set; }

        public SlotType TypeAvailable { get; set; }
        public bool DisabilitySignup { get; set; }
        public bool Closed { get; set; }
        public Location Location { get; set; }

        public Collection<SlotType> SignupTypes { get; set; } // public SlotType SignupType { get; set; }

        public Collection<SignupSlotState> SignupNumbers { get; set; }

        public Group Group { get; set; }

        public string Description { get; set; }

        public bool OverrideClose { get; set; }
        public bool MergeReserve { get; set; }

        public string SlotsAvailableString()
        {
            if (!OverrideClose && Closed)
                return "Sign up closed";

            int placesAvailable;

            if (!MergeReserve)
            {
                var inMain = false;

                var totalMainRemaining = 0;
                var totalReserveRemaining = 0;
                
                foreach (var slot in SignupNumbers)
                {

                    totalMainRemaining += slot.TotalSlotsAvailable - slot.NumberSignedUp < 0 ? 0 : slot.TotalSlotsAvailable - slot.NumberSignedUp;

                    if (totalMainRemaining >= slot.NumberSignedUp)
                    {
                        totalReserveRemaining += slot.TotalReserveAvailable;
                    }
                    else
                    {
                        totalReserveRemaining += (slot.TotalSlotsAvailable + slot.TotalReserveAvailable) - slot.NumberSignedUp;
                    }
                }

                if (totalMainRemaining > 0)
                {

                    return string.Format("{1} {0} Available ({2} Main, {3} Reserve)",
                        "PLACE".SingularOrPlural(TotalSlotsAvailable + TotalReserveAvailable - NumberSignedUp),
                        TotalSlotsAvailable + TotalReserveAvailable - NumberSignedUp,
                        totalMainRemaining,
                        totalReserveRemaining);
                }


                totalReserveRemaining = SignupNumbers.Sum(n => n.TotalReserveAvailable + n.TotalSlotsAvailable - n.NumberSignedUp);

                return string.Format("{1} {0} Available",
                    "RESERVE".SingularOrPlural(totalReserveRemaining - NumberSignedUp),
                    totalReserveRemaining);

                return string.Format("{1} {0} Available", "Place".SingularOrPlural(TotalSlotsAvailable + TotalReserveAvailable - NumberSignedUp), TotalSlotsAvailable + TotalReserveAvailable - NumberSignedUp);


            }

            var AnyMainMerge = SignupNumbers.Any(n => n.NumberSignedUp < n.TotalSlotsAvailable + n.TotalReserveAvailable);


            if (AnyMainMerge)
            {
                var slotsToUse = SignupNumbers.Where(n => n.NumberSignedUp < n.TotalSlotsAvailable + n.TotalReserveAvailable);

                var places = slotsToUse.Sum(n => (n.TotalSlotsAvailable + n.TotalReserveAvailable) - n.NumberSignedUp);

                return string.Format("{1} {0} Available", "PLACE".SingularOrPlural(places), places);
            }

            
            var intPlaces = SignupNumbers.Sum(n => (n.TotalSlotsAvailable + n.TotalInterestedAvaiable + n.TotalReserveAvailable) - n.NumberSignedUp);

            if(intPlaces > 0)
            {
                return string.Format("{1} {0} Available", "INTERESTED", intPlaces);
            }

            return "No Places Available";
        }


        public class SignupSlotState
        {
            public int SlotID { get; set; }
            public int NumberSignedUp { get; set; }
            public int TotalSlotsAvailable { get; set; }
            public int TotalReserveAvailable { get; set; }
            public int TotalInterestedAvaiable { get; set; }
        }
    }

    public static class StringExtensions
    {
        public static string SingularOrPlural(this string _term, int _count)
        {
            return _count == 1 ? _term : _term + "S";
        }
    }
}

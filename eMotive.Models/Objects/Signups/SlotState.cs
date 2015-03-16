namespace eMotive.Models.Objects.Signups
{
    public class SlotState
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public string Time { get; set; }
        public int TotalPlacesAvailable { get; set; }
        public int TotalReserveAvailable { get; set; }
        public int TotalInterestedAvaiable { get; set; }
        public int NumberSignedUp { get; set; }
        public bool Enabled { get; set; }

        public bool OverrideClose { get; set; }
        public bool MergeReserve { get; set; }
        public bool Closed { get; set; }

        public SlotStatus Status { get; set; }
        public SlotType SignupType { get; set; } 

        public int PlacesAvailable()
        {
            return TotalPlacesAvailable - NumberSignedUp;
        }

        public string PlacesAvailableString()
        {
            if (!OverrideClose && Closed)
                return "Sign up closed";

            int placesAvailable;

            if (!MergeReserve)
            {

                if (NumberSignedUp < TotalPlacesAvailable)
                    return string.Format("{1} {0} Available", "PLACE".SingularOrPlural(TotalPlacesAvailable - NumberSignedUp), TotalPlacesAvailable - NumberSignedUp);

                if (NumberSignedUp < TotalPlacesAvailable + TotalReserveAvailable)
                    return string.Format("{1} {0} Available", "RESERVE".SingularOrPlural(TotalPlacesAvailable +TotalReserveAvailable - NumberSignedUp), TotalPlacesAvailable + TotalReserveAvailable - NumberSignedUp);

                placesAvailable = TotalPlacesAvailable + TotalReserveAvailable + TotalInterestedAvaiable - NumberSignedUp;
                if (NumberSignedUp < TotalPlacesAvailable + TotalReserveAvailable + TotalInterestedAvaiable)
                    return string.Format("{1} {0} Available", "INTERESTED", placesAvailable);

            }
            else
            {
                if (NumberSignedUp < TotalPlacesAvailable + TotalReserveAvailable)
                {
                    placesAvailable = TotalPlacesAvailable + TotalReserveAvailable - NumberSignedUp;
                    return string.Format("{1} {0} Available", "PLACE".SingularOrPlural(placesAvailable), placesAvailable);
                }
            }

            if (NumberSignedUp < TotalPlacesAvailable + TotalReserveAvailable + TotalInterestedAvaiable)
            {
                placesAvailable = TotalPlacesAvailable + TotalReserveAvailable + TotalInterestedAvaiable - NumberSignedUp;
                return string.Format("{1} {0} Available", "INTERESTED", placesAvailable);
            }

            return "No Places Available";
        }

        public bool IsSignedUpToSlot()
        {
            return Status == SlotStatus.AlreadySignedUp;
        }
    }
}

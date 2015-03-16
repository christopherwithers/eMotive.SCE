using System;

namespace eMotive.Models.Objects.Signups
{
    public class SessionDay
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public int MainPlaces { get; set; }
        public int PlacesLeft { get; set; }
        public string Group { get; set; }
        public bool Closed { get; set; }
    }
}

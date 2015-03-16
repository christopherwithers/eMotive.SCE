using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using ServiceStack.Common;

namespace eMotive.Models.Objects.SignupsMod
{
    public class Slot
    {
        private int _numberSignedUp = -1;
        private bool? _isSignedUp;

        public int id { get; set; }
        public string Description { get; set; }
        public int PlacesAvailable { get; set; }
        public int ReservePlaces { get; set; }
        public int InterestedPlaces { get; set; }
        public bool Enabled { get; set; }
        public int IdSignUp { get; set; }
        public DateTime Time { get; set; }
        public ICollection<UserSignup> UsersSignedUp { get; set; }

        public bool MergeReserve { get; set; }

        public string SlotsAvailableString { get; set; }

        public bool SignedUp(string username)
        {//TODO: do we need this n.UsersSignedUp.HasContent() ??

            if (_isSignedUp.HasValue)
                return _isSignedUp.Value;

            if (string.IsNullOrEmpty(username))
                return false;

            if (UsersSignedUp.IsEmpty())
            {
                _isSignedUp = false;

                return _isSignedUp.Value;
            }
            _isSignedUp = UsersSignedUp.Any(m => m.User.Username == username);

            return _isSignedUp.Value;
        }

        public int NumberSignedUp()
        {
            if (_numberSignedUp > -1)
                return _numberSignedUp;


            return _numberSignedUp = UsersSignedUp.HasContent() ? UsersSignedUp.Count : 0;
        }


        //TODO:merge reserve(!)
        virtual public SlotType GetUserSignupType(string username)
        {
            //   var userPosition = _slot.UsersSignedUp.ToList().FindIndex(n => n.Type ==)
            // throw new NotImplementedException();

            //    if(_slot)

            if (UsersSignedUp.HasContent())
            {
                var userSignup = UsersSignedUp.SingleOrDefault(n => n.User.Username == username);

                if (userSignup != null)
                {
                    var usersIndex = UsersSignedUp.FindIndex(n => n.User.Username == username) + 1;

                    if (usersIndex <= PlacesAvailable)
                        return SlotType.Main;

                    if (usersIndex <= PlacesAvailable + ReservePlaces)
                        return SlotType.Reserve;

                    return SlotType.Interested;
                }

                //todo: error check incase userSignup is null??
            }

            return SlotType.Interested; //todo: need an error slot?
        }

        public void GeneratePlacesAvailableString()
        {
            int placesAvailable;

            if (!MergeReserve)
            {
                var totalMainRemaining = 0;
                var totalReserveRemaining = 0;

                totalMainRemaining += PlacesAvailable - NumberSignedUp() < 0 ? 0 : PlacesAvailable - NumberSignedUp();

                if (totalMainRemaining >= NumberSignedUp())
                {
                    totalReserveRemaining += ReservePlaces;
                }
                else
                {
                    totalReserveRemaining += (PlacesAvailable + ReservePlaces) - NumberSignedUp();
                }


                if (totalMainRemaining > 0)
                {
                    SlotsAvailableString = string.Format("{1} {0} Available ({2} Main, {3} Reserve)",
                        "PLACE".SingularOrPlural(PlacesAvailable + ReservePlaces - NumberSignedUp()),
                        PlacesAvailable + ReservePlaces - NumberSignedUp(),
                        totalMainRemaining,
                        totalReserveRemaining);
                    return;
                }

                totalReserveRemaining = ReservePlaces + PlacesAvailable - NumberSignedUp();

                SlotsAvailableString = string.Format("{1} {0} Available",
                    "RESERVE".SingularOrPlural(totalReserveRemaining - NumberSignedUp()),
                    totalReserveRemaining);

                SlotsAvailableString = string.Format("{1} {0} Available", "Place".SingularOrPlural(PlacesAvailable + ReservePlaces - NumberSignedUp()), PlacesAvailable + ReservePlaces - NumberSignedUp());
                return;
            }



            if (NumberSignedUp() < PlacesAvailable + ReservePlaces)
            {
                placesAvailable = PlacesAvailable + ReservePlaces - NumberSignedUp();
                SlotsAvailableString = string.Format("{1} {0} Available", "PLACE".SingularOrPlural(placesAvailable), placesAvailable);
                return;
            }


            if (NumberSignedUp() < PlacesAvailable + ReservePlaces + InterestedPlaces)
            {
                placesAvailable = PlacesAvailable + ReservePlaces + InterestedPlaces - NumberSignedUp();
                SlotsAvailableString = string.Format("{1} {0} Available", "INTERESTED", placesAvailable);
                return;
            }

            SlotsAvailableString = "No Places Available";
        }


    }
}

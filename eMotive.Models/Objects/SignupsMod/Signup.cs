using System;
using System.Collections.Generic;
using System.Linq;
using eMotive.Models.Objects.Signups;
using Extensions;

namespace eMotive.Models.Objects.SignupsMod
{
    public class Signup
    {
        private bool? _isSignedUp;

        public Signup()
        {
            _isSignedUp = false;
        }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime CloseDate { get; set; }
        public Group Group { get; set; }
        public Location Location { get; set; }
        
        public string AcademicYear { get; set; }
        public bool Closed { get; set; }
        public bool OverrideClose { get; set; }
        public bool MergeReserve { get; set; }
        public bool AllowMultipleSignups { get; set; }
        public bool IsTraining { get; set; }

        public string Description { get; set; }

        public IEnumerable<Slot> Slots { get; set; }

        public int TotalSlotsAvailable { get; set; }
        public int TotalReserveAvailable { get; set; }
        public int TotalInterestedAvaiable { get; set; }
        public int TotalNumberSignedUp { get; set; }

        public string SlotsAvailableString { get; set; }

        public bool SignedUp(string username)
        {//TODO: do we need this n.UsersSignedUp.HasContent() ??
            _isSignedUp = Slots.Any(n =>  n.SignedUp(username));

            return _isSignedUp.Value;
        }


        public void GenerateSlotsAvailableString()
        {
            if (!OverrideClose && Closed)
                SlotsAvailableString = "Sign up closed";

            TotalSlotsAvailable = Slots.Sum(n => n.PlacesAvailable);
            TotalReserveAvailable = Slots.Sum(n => n.ReservePlaces);
            TotalInterestedAvaiable = Slots.Sum(n => n.InterestedPlaces);
            TotalNumberSignedUp = Slots.Sum(n => n.NumberSignedUp());

            int placesAvailable;
            if (!MergeReserve)
            {
                if (TotalNumberSignedUp >= TotalSlotsAvailable)
                {//if there are more users signed up than there are main spaces available
                    if (TotalNumberSignedUp >= TotalSlotsAvailable + TotalReserveAvailable)
                    {//if more people signed up than main and reserve combined, no places available
                        SlotsAvailableString = "No Places Available";
                        return;
                    }

                    //THere are only reserve places available, display how many
                    var value = (TotalSlotsAvailable + TotalReserveAvailable) - TotalNumberSignedUp;
                    SlotsAvailableString = string.Format("{1} {0} Available", "RESERVE".SingularOrPlural(value), value);
                    return;
                }


                //There are main spots available, show user how many main and reserve are remaining
                SlotsAvailableString = string.Format("{1} {0} Available ({2} Main, {3} Reserve)",
                                        "PLACE".SingularOrPlural(TotalSlotsAvailable + TotalReserveAvailable - TotalNumberSignedUp),
                                        TotalSlotsAvailable + TotalReserveAvailable - TotalNumberSignedUp,
                                        TotalSlotsAvailable - TotalNumberSignedUp,
                                        TotalReserveAvailable);
                return;
            }

            if (TotalNumberSignedUp < TotalSlotsAvailable + TotalReserveAvailable)
            {
                placesAvailable = TotalSlotsAvailable + TotalReserveAvailable - TotalNumberSignedUp;
                SlotsAvailableString = string.Format("{1} {0} Available", "PLACE".SingularOrPlural(placesAvailable), placesAvailable);
                return;
            }


            if (TotalNumberSignedUp < TotalSlotsAvailable + TotalReserveAvailable + TotalInterestedAvaiable)
            {
                placesAvailable = TotalSlotsAvailable + TotalReserveAvailable + TotalInterestedAvaiable - TotalNumberSignedUp;
                SlotsAvailableString = string.Format("{1} {0} Available", "INTERESTED", placesAvailable);
                return;
            }

            SlotsAvailableString = "No Places Available";
        }


        virtual public SlotType GenerateUserSignupType(Slot _slot, int _userId)
        {
            //   var userPosition = _slot.UsersSignedUp.ToList().FindIndex(n => n.Type ==)
            // throw new NotImplementedException();

            //    if(_slot)

            if (_slot.UsersSignedUp.HasContent())
            {
                var userSignup = _slot.UsersSignedUp.SingleOrDefault(n => n.User.ID == _userId);

                if (userSignup != null)
                {
                    var usersIndex = _slot.UsersSignedUp.FindIndex(n => n.User.ID == _userId) + 1;

                    if (usersIndex <= _slot.PlacesAvailable)
                        return SlotType.Main;

                    if (usersIndex <= _slot.PlacesAvailable + _slot.ReservePlaces)
                        return SlotType.Reserve;

                    return SlotType.Interested;
                }

                //todo: error check incase userSignup is null??
            }

            return SlotType.Interested; //todo: need an error slot?
        }
        /*
        virtual public SlotStatus GenerateSlotStatus(Slot _slot, string _username)
        {

            if (!_slot.Enabled)
                return SlotStatus.Closed;

            if (Closed && !OverrideClose)//checked for overide? - centralise closed logic??
                return SlotStatus.Closed;

            var userIsSignnedUpToCurrentSignup = false;
            var applicantsSignedUp = 0;

            if (_slot.UsersSignedUp.HasContent())
            {
                userIsSignnedUpToCurrentSignup = _slot.UsersSignedUp.Any(n => String.Equals(n.User.Username, _username, StringComparison.CurrentCultureIgnoreCase));
                applicantsSignedUp = _slot.UsersSignedUp.Count();
            }

            var currentSignup = _params.UsersSignups.Select(n => n.IdSlot == _slot.ID);

            if (AllowMultipleSignups)
            {
                if (userIsSignnedUpToCurrentSignup)
                    return SlotStatus.AlreadySignedUp;

                if (_params.UserHasSignup && !_params.MultipleSignupsPerSignup)
                    return SlotStatus.Clash;

                return SlotStatus.Signup;
            }


            if (!_params.MultipleSignupsPerSignup)
            {
                if (userIsSignnedUpToCurrentSignup)
                    return SlotStatus.AlreadySignedUp;

                if (_params.UserHasSignup)
                    return SlotStatus.Clash;

                return SlotStatus.Signup;
            }


            if (applicantsSignedUp < _slot.TotalPlacesAvailable)
            {
                return SlotStatus.Signup;
            }

            if (applicantsSignedUp < _slot.TotalPlacesAvailable + _slot.ReservePlaces)
            {
                return _params.MergeReserve ? SlotStatus.Signup : SlotStatus.Reserve;
            }

            if (applicantsSignedUp < _slot.TotalPlacesAvailable + _slot.ReservePlaces + _slot.InterestedPlaces)
                return SlotStatus.Interested;

            if (applicantsSignedUp >= _slot.TotalPlacesAvailable + _slot.ReservePlaces + _slot.InterestedPlaces)
                return SlotStatus.Full;

            /*if (_userSignup != null)
                return SlotStatus.Clash;*/
        /*
            if (!_slot.ApplicantsSignedUp.HasContent())
                return SlotStatus.Signup;



            return SlotStatus.Signup;//todo: need ERROR here?

        }

    */

    }

    public static class StringExtensions
    {
        public static string SingularOrPlural(this string _term, int _count)
        {
            return _count == 1 ? _term : _term + "S";
        }

        public static int FindIndex<T>(this IEnumerable<T> list, Predicate<T> finder)
        {
            int index = 0;
            foreach (var item in list)
            {

                if (finder(item))
                {
                    return index;
                }
                index++;
                //  index++;
            }

            return -1;
        }
    }



}

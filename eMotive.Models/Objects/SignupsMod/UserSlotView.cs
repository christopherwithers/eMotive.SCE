using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Extensions;

namespace eMotive.Models.Objects.SignupsMod
{
    public class UserSlotView
    {
        public Dictionary<int, SlotType> SignupStatus;

        public string LoggedInUser { get; set; }

        public string HeaderText { get; set; }
        public string FooterText { get; set; }

        public Signup Signup { get; set; }

        public void Initialise(string username)
        {
            SignupStatus = new Dictionary<int, SlotType>();
            foreach (var slot in Signup.Slots ?? new Slot[] { })
            {
                slot.GeneratePlacesAvailableString();

                if (slot.SignedUp(username))
                {
                    SignupStatus.Add(slot.id, slot.GetUserSignupType(username));
                }

            }
        }

        public string SlotStatus(int _id)
        {
            SlotType type;
            return SignupStatus.TryGetValue(_id, out type) ? type.ToString() : string.Empty;
        }

        public string HomeViewRowStyle(int _slotID)
        {
            SlotType type;
            if (!SignupStatus.TryGetValue(_slotID, out type))
                return string.Empty;

            switch (type)
            {
                case SlotType.Main:
                case SlotType.Reserve:
                    return "success";
                case SlotType.Interested:
                    return "info";
                default:
                    return string.Empty;
            }
        }

        public string HomeViewRowBadge(int _slotID)
        {
            SlotType type;
            if (!SignupStatus.TryGetValue(_slotID, out type))
                return string.Empty;

            switch (type)
            {
                case SlotType.Main:
                    return "<span class='label label-success'>Signed Up</span> <span class='label label-success' style='clear: left;'>Main</span>";
                case SlotType.Reserve:
                    return "<span class='label label-success'>Signed Up</span> <span class='label label-success' style='clear: left;'>Reserve</span>";
                case SlotType.Interested:
                    return "<span class='label label-success'>Signed Up</span> <span class='label label-info' style='clear: left;'>Interested</span>";
                default:
                    return string.Empty;
            }
        }

        /*        public static string SetStatusStyle(int _totalplaces, int _remainingPlaces, bool _signedUp)
        {
            if (_signedUp)
                return "success";

            var placesLeft = (100 * _remainingPlaces) / _totalplaces;

            if (placesLeft <= 10)
                return "danger";
            
            if (placesLeft <= 30)
                return"warning";
            
            if (placesLeft <= 60)
                return "info";

            return string.Empty;
        }*/


        public string HomeViewRowButton(int _slotID)
        {//TODO: merge as one style? add 'btn' auctomatically?
            SlotType type;
            if (SignupStatus.TryGetValue(_slotID, out type))
                return string.Empty;

            switch (type)
            {
                case SlotType.Main:
                case SlotType.Reserve:
                    return "btn btn-success";
                case SlotType.Interested:
                    return "btn btn-info";
                default:
                    return string.Empty;
            }
        }

        public string StatusStyle(int _slotID)
        {//TODO: merge as one style? add 'btn' auctomatically?
            SlotType type;
            if (SignupStatus.TryGetValue(_slotID, out type))
            {
                switch (type)
                {
                    case SlotType.Main:
                    case SlotType.Reserve:
                        return "success";
                    case SlotType.Interested:
                        return "info";
                    default:
                        return string.Empty;
                }
            }

            var slot = Signup.Slots.SingleOrDefault(n => n.id == _slotID);

            if (slot == null)
                return string.Empty;

            var totalPlaces = slot.PlacesAvailable + slot.ReservePlaces + slot.InterestedPlaces;

            var placesLeft = 100 * ((totalPlaces - slot.NumberSignedUp()) / totalPlaces);

            if (placesLeft <= 10)
                return "error"; //"danger"

            if (placesLeft <= 30)
                return "warning";

            if (placesLeft <= 60)
                return "info";

            return string.Empty;
        }

        public string SlotStatusName(int _slotID)
        {
            switch (GenerateSlotStatus(_slotID))
            {
                case Objects.SlotStatus.AlreadySignedUp:
                    return "Cancel My Appointment";

                case Objects.SlotStatus.Clash:
                    return "Unavailable";
                    break;
                case Objects.SlotStatus.Closed:
                    return "Closed";
                /*    case SlotStatus.SignupClosed:
                        name = "Closed";
                        break;
                    case SlotStatus.AlreadySignedUpClosed:
                        name = "Closed";
                        break;*/
                case Objects.SlotStatus.Full:
                    return "Full";
                case Objects.SlotStatus.Signup:
                    return "Sign Up";
                case Objects.SlotStatus.Interested:
                    return "Interested";
                case Objects.SlotStatus.Reserve:
                    return "Reserve";
                default:
                    return "Error";
            }
        }

        public SlotStatus GenerateSlotStatus(int _slotID)
        {
            var slot = Signup.Slots.SingleOrDefault(n => n.id == _slotID);

            if (!slot.Enabled)
                return Objects.SlotStatus.Closed;

            if (Signup.Closed && !Signup.Closed)//checked for overide? - centralise closed logic??
                return Objects.SlotStatus.Closed;

            var userIsSignnedUpToCurrentSignup = false;
            var applicantsSignedUp = 0;

            if (slot.UsersSignedUp.HasContent())
            {
                userIsSignnedUpToCurrentSignup = slot.UsersSignedUp.Any(n => String.Equals(n.User.Username, LoggedInUser, StringComparison.CurrentCultureIgnoreCase));
                applicantsSignedUp = slot.UsersSignedUp.Count();
            }

        //    var currentSignup = slot.UsersSignups.Select(n => n.IdSlot == _slot.ID);

            if (!Signup.Group.AllowMultipleSignups)
            {
                if (userIsSignnedUpToCurrentSignup)
                    return Objects.SlotStatus.AlreadySignedUp;

                //  if (_params.UserHasSignup && !Signup.Group.AllowMultipleSignups)
                 //   return SlotStatus.Clash;

                return Objects.SlotStatus.Signup;
            }


            if (!Signup.AllowMultipleSignups)
            {
                if (userIsSignnedUpToCurrentSignup)
                    return Objects.SlotStatus.AlreadySignedUp;

                //if (_params.UserHasSignup)
                //    return SlotStatus.Clash;

                return Objects.SlotStatus.Signup;
            }


            if (applicantsSignedUp < slot.PlacesAvailable)
            {
                return Objects.SlotStatus.Signup;
            }

            if (applicantsSignedUp < slot.PlacesAvailable + slot.ReservePlaces)
            {
                return Signup.MergeReserve ? Objects.SlotStatus.Signup : Objects.SlotStatus.Reserve;
            }

            if (applicantsSignedUp < slot.PlacesAvailable + slot.ReservePlaces + slot.InterestedPlaces)
                return Objects.SlotStatus.Interested;

            if (applicantsSignedUp >= slot.PlacesAvailable + slot.ReservePlaces + slot.InterestedPlaces)
                return Objects.SlotStatus.Full;

            /*if (_userSignup != null)
                return SlotStatus.Clash;*/

            if (!slot.UsersSignedUp.HasContent())
                return Objects.SlotStatus.Signup;



            return Objects.SlotStatus.Signup;//todo: need ERROR here?

        }

        public string AssignStatusFunctionality(int _slotID, string _username)
        {
            switch (GenerateSlotStatus(_slotID))
            {
                case Objects.SlotStatus.AlreadySignedUp:
                    return string.Format("DoCancelSignup({0},{1},\"{2}\");", Signup.Id, _slotID, _username);
                case Objects.SlotStatus.Clash:
                    return "ShowClashModal();";
                case Objects.SlotStatus.Closed:
                    return "ShowDateClosedModal();";
                case Objects.SlotStatus.Full:
                    return "ShowFullModal();";
                case Objects.SlotStatus.Signup:
                case Objects.SlotStatus.Reserve:
                case Objects.SlotStatus.Interested:
                    return string.Format("DoSignup({0},{1},\"{2}\");", Signup.Id, _slotID, _username);
                default:
                    return "alert(\"error!\"); return false;";

            }
        }



        public string AssignStatusFunctionality(int _slotID)
        {
            return AssignStatusFunctionality(_slotID, LoggedInUser);
        }
    }
}

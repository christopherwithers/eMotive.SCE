using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Extensions;

namespace eMotive.Models.Objects.SignupsMod
{
    public class UserSignupView
    {
        private IDictionary<string, List<Signup>> _signupsByGroup;
     //   private Collection<SlotType> _userSignupTypes;

        public string LoggedInUser { get; set; }

        public string HeaderText { get; set; }
        public string FooterText { get; set; }
        
        public IEnumerable<Signup> Signups { get; set; }

        public List<Tuple<int, SlotType>> SignedInUserSlotTypes { get; set; }


        public IDictionary<string, List<Signup>> GetSignupsByGroup()
        {
            if (!Signups.HasContent())
                return null;

            if (_signupsByGroup.HasContent())
                return _signupsByGroup;

            return _signupsByGroup = Signups.GroupBy(m => m.Group.Name).ToDictionary(k => k.Key, v => v.ToList());
        }

        public string GetDescription(string key)
        {
            return Signups.First(n => n.Group.Name == key).Group.Description;
        }
        /*
        public Collection<SlotType> UserSignupTypes()
        {
            if (_userSignupTypes.HasContent())
                return _userSignupTypes;

            _userSignupTypes = new Collection<SlotType>();

            foreach (var signup in Signups)
            {
                foreach (var slot in signup.Slots)
                {
                    if (slot.UsersSignedUp.HasContent())
                    {
                        var userSignup = slot.UsersSignedUp.SingleOrDefault(n => n.User.Username == LoggedInUser);

                        if (userSignup != null)
                        {
                            var usersIndex = slot.UsersSignedUp.FindIndex(n => n.User.Username == LoggedInUser) + 1;

                            if (usersIndex <= slot.PlacesAvailable)
                            {
                                _userSignupTypes.Add(SlotType.Main);
                                continue;
                            }

                            if (usersIndex <= slot.PlacesAvailable + slot.ReservePlaces)
                            {
                                _userSignupTypes.Add(SlotType.Reserve);
                                continue;
                            }

                            _userSignupTypes.Add(SlotType.Interested);
                        }
                    }
                }

            //todo: error check incase userSignup is null??
            }

            return _userSignupTypes;
        }*/

        public void Initialise(string username)
        {
            SignedInUserSlotTypes = new List<Tuple<int, SlotType>>();

            foreach (var signup in Signups ?? new Signup[] {})
            {
                signup.GenerateSlotsAvailableString();
             
                foreach (var slot in signup.Slots)
                {
                    if (slot.SignedUp(username))
                    {
                        SignedInUserSlotTypes.Add(new Tuple<int, SlotType>(signup.Id, slot.GetUserSignupType(username)));
                    }
                }
            }
        }

    }
}

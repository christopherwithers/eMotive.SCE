using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using Cache.Interfaces;
using eMotive.Managers.Objects.Search;
using eMotive.Managers.Objects.Signups;
using eMotive.Models.Objects;
using eMotive.Models.Objects.Search;
using eMotive.Models.Objects.Users;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;
using Extensions;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Signups;
using eMotive.Repository.Interfaces;
using eMotive.Services.Interfaces;
using Lucene.Net.Search;
using rep = eMotive.Repository.Objects.Signups;
using emSearch = eMotive.Search.Objects.Search;
using Group = eMotive.Models.Objects.Signups.Group;

namespace eMotive.Managers.Objects
{
    public class SessionManager : ISessionManager
    {
        private readonly ISessionRepository signupRepository;
        private readonly IUserManager userManager;
        private readonly ISearchManager searchManager;
        private readonly IFormManager formManager;

        public SessionManager(ISessionRepository _signupRepository, IUserManager _userManager, ISearchManager _searchManager, IFormManager _formManager)
        {
            signupRepository = _signupRepository;
            userManager = _userManager;
            searchManager = _searchManager;
            formManager = _formManager;

            AutoMapperManagerConfiguration.Configure();
        }

        public IeMotiveConfigurationService configurationService { get; set; }
        public INotificationService notificationService { get; set; }
        public IEmailService emailService { get; set; }
        public ICache cache { get; set; }

        readonly Dictionary<int, object> dictionary = new Dictionary<int, object>();
        readonly object dictionaryLock = new object();

        /*private IEnumerable<Repository.Objects.Signups.Signup> FetchSignupsByGroup(IEnumerable<int> _groups)
        {
            var cacheId = string.Format("RepSignups_{0}", string.Join("_", _groups));

            var signups = cache.FetchCollection<Repository.Objects.Signups.Signup>(cacheId, n => n.id, null);

            if (signups.HasContent())
                return signups;

            signups = signupRepository.FetchSignupsByGroup(_groups);

            cache.PutCollection(signups, n => n.id, cacheId);

            return signups;
        }*/

        private IEnumerable<Signup> FetchSignupsByGroup(IEnumerable<int> _groups)
        {
 
           // signups = 

           // var cacheId = "ModelSignupCollection";

           // var signupModels = cache.FetchCollection<Signup>(cacheId, i => i.ID, null);//TODO: need a fetch (ids) func to push in here?

           // if (signupModels.HasContent())
           //     return signupModels;

            var signups = signupRepository.FetchSignupsByGroup(_groups);

            if (!signups.HasContent())
                return null;

            var signupModels = Mapper.Map<IEnumerable<rep.Signup>, IEnumerable<Signup>>(signups);

            var usersDict = userManager.Fetch(signups.SelectMany(u => u.Slots.Where(n => n.UsersSignedUp.HasContent()).SelectMany(n => n.UsersSignedUp).Select(m => m.IdUser))).ToDictionary(k => k.ID, v => v);

            var locationDict = formManager.FetchFormList("Sites")
                .Collection.ToDictionary(k => k.Value, v => new Location { ID = v.Value, Name = v.Text });

            foreach (var repSignup in signups)
            {
                foreach (var modSignup in signupModels)
                {
                    if (repSignup.id == modSignup.ID)
                        modSignup.Location = locationDict[repSignup.idSite.ToString(CultureInfo.InvariantCulture)];
                    foreach (var repSlot in repSignup.Slots)
                    {
                        foreach (var slot in modSignup.Slots)
                        {
                            if (repSlot.id != slot.ID) continue;

                            if (!repSlot.UsersSignedUp.HasContent()) continue;

                            slot.ApplicantsSignedUp = new Collection<UserSignup>();
                            foreach (var user in repSlot.UsersSignedUp)
                            {
                                slot.ApplicantsSignedUp.Add(new UserSignup { User = usersDict[user.IdUser], SignupDate = user.SignUpDate, ID = user.ID });
                            }
                        }
                    }
                }
            }

            //cache.PutCollection(signupModels, i => i.ID, cacheId);

            return signupModels;
        }

        //todo: closed date logic here
        public Signup Fetch(int _id)
        {
            var cacheId = string.Format("ModelSignup_{0}", _id);

            var signup = cache.FetchItem<Signup>(cacheId);

            if (signup != null)
                return signup;

            var repSignup = signupRepository.Fetch(_id);


            if (repSignup == null)
            {
                notificationService.AddError("The requested signup could not be found.");
                return null;
            }

            signup = Mapper.Map<rep.Signup, Signup>(repSignup);

            var locationDict = formManager.FetchFormList("Sites")
                .Collection.ToDictionary(k => k.Value, v => new Location { ID = v.Value, Name = v.Text });

            IDictionary<int, User> usersDict = null;

            signup.Location = locationDict[repSignup.idSite.ToString(CultureInfo.InvariantCulture)];
            if (repSignup.Slots.Any(n => n.UsersSignedUp.HasContent()))
            {
                //  usersDict = new Dictionary<int, User>();
                var UsersSignedUp = repSignup.Slots.Where(n => n.UsersSignedUp != null).SelectMany(m => m.UsersSignedUp);//.Select(u => u.IdUser);
                var userIds = UsersSignedUp.Select(u => u.IdUser);
                var users = userManager.Fetch(userIds);
                usersDict = users.ToDictionary(k => k.ID, v => v);
            }

            foreach (var repSlot in repSignup.Slots)
            {
                foreach (var slot in signup.Slots)
                {
                    if (repSlot.id != slot.ID) continue;

                    if (!repSlot.UsersSignedUp.HasContent()) continue;

                    slot.ApplicantsSignedUp = new Collection<UserSignup>();
                    foreach (var user in repSlot.UsersSignedUp)
                    {
                        slot.ApplicantsSignedUp.Add(new UserSignup { User = usersDict[user.IdUser], SignupDate = user.SignUpDate, ID = user.ID });
                    }
                }
            }


            return signup;
        }


        public Signup Fetch(int[] _ids)
        {
            throw new NotImplementedException();
        }


        //TODO: need a new signup admin obj which contains full user + signup date etc! Then map to it!
        public IEnumerable<Signup> FetchAll()
        {
            var cacheId = "ModelSignupCollection";

            var signupModels = cache.FetchCollection<Signup>(cacheId, i => i.ID, null);//TODO: need a fetch (ids) func to push in here?

            if (signupModels.HasContent())
                return signupModels;

            var signups = signupRepository.FetchAll();

            if (!signups.HasContent())
                return null;

            signupModels = Mapper.Map<IEnumerable<rep.Signup>, IEnumerable<Signup>>(signups);

            var usersDict = userManager.Fetch(signups.SelectMany(u => u.Slots.Where(n => n.UsersSignedUp.HasContent()).SelectMany(n => n.UsersSignedUp).Select(m => m.IdUser))).ToDictionary(k => k.ID, v => v);

            var locationDict = formManager.FetchFormList("Sites")
                .Collection.ToDictionary(k => k.Value, v => new Location { ID = v.Value, Name = v.Text });

            foreach (var repSignup in signups)
            {
                foreach (var modSignup in signupModels)
                {
                    if (repSignup.id == modSignup.ID)
                        modSignup.Location = locationDict[repSignup.idSite.ToString(CultureInfo.InvariantCulture)];
                    foreach (var repSlot in repSignup.Slots)
                    {
                        foreach (var slot in modSignup.Slots)
                        {
                            if (repSlot.id != slot.ID) continue;

                            if (!repSlot.UsersSignedUp.HasContent()) continue;

                            slot.ApplicantsSignedUp = new Collection<UserSignup>();
                            foreach (var user in repSlot.UsersSignedUp)
                            {
                                slot.ApplicantsSignedUp.Add(new UserSignup { User = usersDict[user.IdUser], SignupDate = user.SignUpDate, ID = user.ID });
                            }
                        }
                    }
                }
            }

            cache.PutCollection(signupModels, i => i.ID, cacheId);

            return signupModels;
        }

        public IEnumerable<Group> FetchGroups(IEnumerable<int> _ids)
        {
            return Mapper.Map<IEnumerable<rep.Group>, IEnumerable<Group>>(signupRepository.FetchGroups(_ids));
        }

        public bool Save(Models.Objects.SignupsMod.Signup signup)
        {
            var repSignup = Mapper.Map<Models.Objects.SignupsMod.Signup, rep.Signup>(signup);
            //repSignup.idSite = signup.
            var success = signupRepository.Save(repSignup);

            if (!success)
                notificationService.AddIssue("The session changes could not be saved.");

            return success;
        }

        public bool StandDownExaminers()
        {
            /*    var signups = FetchAll();

                  if (signups.HasContent())
                  {
                      var filtered = signups.Where(n => n.Group.EnableEmails);

                      if (filtered.HasContent())
                      {
                          foreach (var signup in filtered)
                          {
                              foreach (var slot in signup.Slots)
                              {
                                  var replacements = new Dictionary<string, string>(4)
                                  {
                                      {"#forename#", user.Forename},
                                      {"#surname#", user.Surname},
                                      {"#SignupDate#", signup.Date.ToString("dddd d MMMM yyyy")},
                                      {"#SlotDescription#", slot.Description},
                                      {"#SignupDescription#", signup.Description},
                                      {"#GroupDescription#", signup.Group.Name},
                                      {"#username#", user.Username},
                                      {"#sitename#", configurationService.SiteName()},
                                      {"#siteurl#", configurationService.SiteURL()}
                                  };

                                  if (emailService.SendMail("StandDownExaminers" user.Email, replacements))
                                  {
                                      emailService.SendEmailLog(key, user.Username);
                                      return true;
                                  }
                              }
                          }
                      }
            
                
                  }*/

            return false;
        }

        public IEnumerable<Signup> FetchAllTraining()
        {
            var cacheId = "ModelSignupCollectionTraining";

            var signupModels = cache.FetchCollection<Signup>(cacheId, i => i.ID, null);//TODO: need a fetch (ids) func to push in here?

            if (signupModels.HasContent())
                return signupModels;

            var signups = signupRepository.FetchAllTraining();

            if (!signups.HasContent())
                return null;

            signupModels = Mapper.Map<IEnumerable<rep.Signup>, IEnumerable<Signup>>(signups);

            var usersDict = userManager.Fetch(signups.SelectMany(u => u.Slots.Where(n => n.UsersSignedUp.HasContent()).SelectMany(n => n.UsersSignedUp).Select(m => m.IdUser))).ToDictionary(k => k.ID, v => v);

            var locationDict = formManager.FetchFormList("Sites")
                .Collection.ToDictionary(k => k.Value, v => new Location { ID = v.Value, Name = v.Text });
            foreach (var repSignup in signups)
            {
                foreach (var modSignup in signupModels)
                {

                    if (repSignup.id == modSignup.ID)
                        modSignup.Location = locationDict[repSignup.idSite.ToString(CultureInfo.InvariantCulture)];

                    foreach (var repSlot in repSignup.Slots)
                    {
                        foreach (var slot in modSignup.Slots)
                        {
                            if (repSlot.id != slot.ID) continue;

                            if (!repSlot.UsersSignedUp.HasContent()) continue;

                            slot.ApplicantsSignedUp = new Collection<UserSignup>();
                            foreach (var user in repSlot.UsersSignedUp)
                            {
                                slot.ApplicantsSignedUp.Add(new UserSignup { User = usersDict[user.IdUser], SignupDate = user.SignUpDate, ID = user.ID });
                            }
                        }
                    }
                }
            }

            cache.PutCollection(signupModels, i => i.ID, cacheId);

            return signupModels;
        }

        public IEnumerable<SessionDay> FetchAllBrief()
        {
            var groups = signupRepository.FetchGroups();

            if (!groups.HasContent())
            {
                notificationService.AddError("An error occurred. Groups could not be found.");
                return null;
            }

            var signups = FetchAll();

            return Mapper.Map<IEnumerable<Signup>, IEnumerable<SessionDay>>(signups);
        }


        public IEnumerable<Group> FetchAllGroups()
        {
            return Mapper.Map<IEnumerable<rep.Group>, IEnumerable<Group>>(signupRepository.FetchGroups());
        }

        public IEnumerable<Models.Objects.SignupsMod.Signup> FetchRecordsFromSearch(SearchResult _searchResult)
        {
            return FetchM(_searchResult.Items.Select(n => n.ID).ToList());
        }

        public bool WillingToChangeSignup(WillingToChangeSignup change)
        {
            return signupRepository.WillingToChangeSignup(Mapper.Map<WillingToChangeSignup, rep.WillingToChangeSignup>(change));
        }

        public IEnumerable<WillingToChangeSignup> FetchWillingToChangeForSignup(int signupID)
        {
            return Mapper.Map<IEnumerable<rep.WillingToChangeSignup>, IEnumerable<WillingToChangeSignup>>(signupRepository.FetchWillingToChangeForSignup(signupID));
        }

        public IEnumerable<WillingToChangeSignup> FetchWillingToChangeForUser(int userID)
        {
            return Mapper.Map<IEnumerable<rep.WillingToChangeSignup>, IEnumerable<WillingToChangeSignup>>(signupRepository.FetchWillingToChangeForUser(userID));
        }

        public UserHomeView FetchHomeView(string _username)
        {
            //todo: fetch user and group
            var user = userManager.Fetch(_username);

            if (user == null)
            {
                //TODo: ERROR MESSAGE HERE!
                return null;
            }

            var profile = userManager.FetchProfile(_username);

            if (!profile.Groups.HasContent())
                return null;

            var signups = FetchSignupsByGroup(profile.Groups.Select(n => n.ID));

            if (signups == null)
                return null;

            var homeView = new UserHomeView
            {
                User = user
            };

            var locationDict = formManager.FetchFormList("Sites")
                .Collection.ToDictionary(k => k.Value, v => new Location { ID = v.Value, Name = v.Text });
            foreach (var signup in signups)
            {
                if (!signup.Slots.HasContent())
                    continue;

                foreach (var slot in signup.Slots)
                {
                    if (!slot.ApplicantsSignedUp.HasContent())
                        continue;

                    foreach (var userSignup in slot.ApplicantsSignedUp)
                    {
                        if (userSignup.User.ID != user.ID) continue;

                        var signupDetails = new UserSignupDetails
                        {
                            SignUpDate = signup.Date.AddHours(slot.Time.Hour).AddMinutes(slot.Time.Minute),
                            SignUpDetails = slot.Description,
                            SignedUpSlotID = slot.ID,
                            SignupID = signup.ID,
                            Location = signup.Location,//locationDict[signup.idSite.ToString(CultureInfo.InvariantCulture)],
                            SignupGroup = new Group { Description = signup.Group.Description, AllowMultipleSignups = signup.Group.AllowMultipleSignups, ID = signup.Group.ID, Name = signup.Group.Name, AllowSelfSignup = signup.Group.AllowSelfSignup },
                            SignupDescription = signup.Description,
                            Type = GenerateUserSignupType(slot, user.ID)// slot.UsersSignedUp.Where(n => n.IdUser == user.ID).Single(t => t.Type)
                        };

                        homeView.SignupDetails.Add(signupDetails);
                    }
                }
            }

            return homeView;
        }

        public bool RegisterAttendanceToSession(SessionAttendance _session)
        {
            return signupRepository.RegisterAttendanceToSession(Mapper.Map<SessionAttendance, rep.SessionAttendance>(_session));
        }


        public UserSignupView FetchSignupInformation(string _username)
        {
            var signupCollection = new Collection<SignupState>();
            var user = userManager.Fetch(_username);

            if (user == null)
            {//TODO: ERROR MESSAGE HERE!!
                return null;
            }

            var profile = userManager.FetchProfile(_username);
            var userSignUp = FetchuserSignups(user.ID, profile.Groups.Select(n => n.ID));
            var signups = FetchSignupsByGroup(profile.Groups.Select(n => n.ID));

            bool signedup = false;
            int signupId = 0;
            if (signups.HasContent())
            {
              //  var locationDict = formManager.FetchFormList("Sites")
               //     .Collection.ToDictionary(k => k.Value, v => new Location { ID = v.Value, Name = v.Text });
                //signupCollection
                foreach (var item in signups)
                {
                    //Logic to deal with applicants and closed signups
                    //if a signup is closed, we hide it from applicants UNLESS they are signed up to a slot in that signup
                    if (!item.Closed || userSignUp != null && userSignUp.Any(n => n.IdSignUp == item.ID))
                    {
                        var signup = new SignupState
                        {
                            ID = item.ID,
                            Date = item.Date,
                            SignedUp =
                                item.Slots.Any(
                                    n =>
                                    n.ApplicantsSignedUp.HasContent() &&
                                    n.ApplicantsSignedUp.Any(m => m != null && m.User.ID == user.ID)),
                            TotalSlotsAvailable = item.Slots.Sum(n => n.TotalPlacesAvailable),
                            TotalReserveAvailable = item.Slots.Sum(n => n.ReservePlaces),
                            TotalInterestedAvaiable = item.Slots.Sum(n => n.InterestedPlaces),

                            NumberSignedUp = item.Slots.Sum(n => n.ApplicantsSignedUp.HasContent() ? n.ApplicantsSignedUp.Count() : 0),

                            MergeReserve = item.MergeReserve,
                            OverrideClose = item.OverrideClose,
                        //    DisabilitySignup = item.Group.,
                            Closed = item.Closed || item.CloseDate < DateTime.Now,
                            Description = item.Description,
                            //       SignupType = item.
                            Group = new Group { AllowMultipleSignups = item.Group.AllowMultipleSignups, Description = item.Group.Description, ID = item.Group.ID, Name = item.Group.Name },
                            Location = item.Location //locationDict[item.idSite.ToString(CultureInfo.InvariantCulture)]
                        };


                        foreach (var slot in item.Slots ?? new Slot[] { })
                        {
                            signup.SignupNumbers.Add(new SignupState.SignupSlotState
                            {
                                SlotID = slot.ID,
                                TotalSlotsAvailable = slot.TotalPlacesAvailable,
                                TotalInterestedAvaiable = slot.InterestedPlaces,
                                TotalReserveAvailable = slot.ReservePlaces,
                                NumberSignedUp = slot.ApplicantsSignedUp.HasContent() ? slot.ApplicantsSignedUp.Count : 0
                            });
                        }

                        if (signup.SignedUp)
                        {
                            signup.SignupTypes = new Collection<SlotType>();
                            signedup = true;
                            signupId = signup.ID;
                            // var usersSlots = item.Slots.Where(n => n.UsersSignedUp.HasContent()).Select(m => m.UsersSignedUp.Where(o => o.IdUser == user.ID));

                            foreach (var userSignup in item.Slots)
                            {
                                if (userSignup.ApplicantsSignedUp.HasContent() && userSignup.ApplicantsSignedUp.Any(n => n.User.ID == user.ID))
                                {
                                    //   var usersIndex = userSignup.UsersSignedUp.FindIndex(n => n.IdUser == user.ID);

                                    // if (usersIndex != null)
                                    // {




                                    signup.SignupTypes.Add(GenerateUserSignupType(userSignup, user.ID));//usersIndex.ToString());
                                    // }
                                }
                            }
                        }

                        signupCollection.Add(signup);
                    }
                }
            }

            var signupView = new UserSignupView
            {
                SignupInformation = signupCollection,
                SignupID = signupId,
                SignedUp = signedup,
            };

            return signupView;

        }

        public UserSignupView FetchSignupInformation(string _username, int _idGroup)
        {
            var signupCollection = new Collection<SignupState>();
            var user = userManager.Fetch(_username);

            if (user == null)
            {//TODO: ERROR MESSAGE HERE!!
                return null;
            }

            var profile = userManager.FetchProfile(_username);


            if (profile.Groups.All(n => n.ID != _idGroup))
            {
                notificationService.AddError("You do not have access to the requested session signup.");
                return null;
            }

            var userSignUp = FetchuserSignups(user.ID, profile.Groups.Select(n => n.ID));
            var signups = FetchSignupsByGroup(profile.Groups.Select(n => n.ID));

            bool signedup = false;
            int signupId = 0;

            if (signups.HasContent())
            {
             //   var locationDict = formManager.FetchFormList("Sites")
                //    .Collection.ToDictionary(k => k.Value, v => new Location { ID = v.Value, Name = v.Text });
                //signupCollection
                foreach (var item in signups)
                {
                    //Logic to deal with applicants and closed signups
                    //if a signup is closed, we hide it from applicants UNLESS they are signed up to a slot in that signup
                    if (!item.Closed || userSignUp != null && userSignUp.Any(n => n.IdSignUp == item.ID))
                    {
                        var signup = new SignupState
                        {
                            ID = item.ID,
                            Date = item.Date,
                            SignedUp =
                                item.Slots.Any(
                                    n =>
                                    n.ApplicantsSignedUp.HasContent() &&
                                    n.ApplicantsSignedUp.Any(m => m != null && m.User.ID == user.ID)),
                            TotalSlotsAvailable = item.Slots.Sum(n => n.TotalPlacesAvailable),
                            TotalReserveAvailable = item.Slots.Sum(n => n.ReservePlaces),
                            TotalInterestedAvaiable = item.Slots.Sum(n => n.InterestedPlaces),
                            NumberSignedUp = item.Slots.Sum(n => n.ApplicantsSignedUp.HasContent() ? n.ApplicantsSignedUp.Count() : 0),
                            MergeReserve = item.MergeReserve,
                            OverrideClose = item.OverrideClose,
                        //    DisabilitySignup = item.Group.DisabilitySignups,
                            Closed = item.Closed || item.CloseDate < DateTime.Now,
                            Description = item.Description,
                            Group = new Group { AllowMultipleSignups = item.Group.AllowMultipleSignups, Description = item.Group.Description, ID = item.Group.ID, Name = item.Group.Name },
                            Location = item.Location//locationDict[item.idSite.ToString(CultureInfo.InvariantCulture)]
                        };

                        if (signup.SignedUp)
                        {
                            signedup = true;
                            signupId = signup.ID;
                        }

                        signupCollection.Add(signup);
                    }
                }
            }

            var signupView = new UserSignupView
            {
                SignupInformation = signupCollection,
                SignupID = signupId,
                SignedUp = signedup
            };

            return signupView;

        }


        public IEnumerable<SignupState> FetchSignupStates(string _username)
        {
            var signupCollection = new Collection<SignupState>();
            var user = userManager.Fetch(_username);

            if (user == null)
            {//TODO: ERROR MESSAGE HERE!!
                return null;
            }

            var profile = userManager.FetchProfile(_username);
            var userSignUp = FetchuserSignup(user.ID, profile.Groups.Select(n => n.ID));
            var signups = FetchSignupsByGroup(profile.Groups.Select(n => n.ID));

            bool signedup = false;
            int signupId = 0;

            if (signups.HasContent())
            {
             //   var locationDict = formManager.FetchFormList("Sites")
                  //  .Collection.ToDictionary(k => k.Value, v => new Location { ID = v.Value, Name = v.Text });
                //signupCollection
                foreach (var item in signups)
                {
                    //Logic to deal with applicants and closed signups
                    //if a signup is closed, we hide it from applicants UNLESS they are signed up to a slot in that signup
                    if (!item.Closed || userSignUp != null && userSignUp.IdSignUp == item.ID)
                    {
                        var signup = new SignupState
                        {
                            ID = item.ID,
                            Date = item.Date,
                            SignedUp =
                                item.Slots.Any(
                                    n =>
                                    n.ApplicantsSignedUp.HasContent() &&
                                    n.ApplicantsSignedUp.Any(m => m != null && m.User.ID == user.ID)),
                            TotalSlotsAvailable = item.Slots.Sum(n => n.TotalPlacesAvailable),
                            TotalReserveAvailable = item.Slots.Sum(n => n.ReservePlaces),
                            TotalInterestedAvaiable = item.Slots.Sum(n => n.InterestedPlaces),
                            NumberSignedUp = item.Slots.Sum(n => n.ApplicantsSignedUp.HasContent() ? n.ApplicantsSignedUp.Count() : 0),
                            MergeReserve = item.MergeReserve,
                            OverrideClose = item.OverrideClose,
                         //   DisabilitySignup = item.Group.DisabilitySignups,
                            Closed = item.Closed || item.CloseDate < DateTime.Now,
                            Location = item.Location//locationDict[item.idSite.ToString(CultureInfo.InvariantCulture)]
                        };

                        if (signup.SignedUp)
                        {
                            signedup = true;
                            signupId = signup.ID;
                        }

                        signupCollection.Add(signup);
                    }
                }
            }

            return signupCollection;
        }

        public UserSlotView FetchSlotInformation(int _signup, string _username)
        {
            var user = userManager.Fetch(_username);

            //TODO need id of slot!
            var signup = Fetch(_signup);
            
           var userProfile = userManager.FetchProfile(_username);

            var hasAccess = userProfile.Groups.Any(@group => signup.Group.ID == @group.ID);

            if (!hasAccess) //signupGroup.ID != 
            {
                notificationService.AddError("You do not have permission to view the requested interview.");
                return null;
            }

            if (signup == null)
            {
                notificationService.AddError("The requested interview date could not be found.");
                return null;
            }

            var userSignups = signupRepository.FetchSignupsForUser(user.ID).ToArray();

            var slotCollection = new Collection<SlotState>();
            var slotView = new UserSlotView(signup);
            var isAdmin = user.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin" || n.Name == "UGC");

            // var groupsignups = signupRepository.f
            foreach (var item in signup.Slots)
            {
                var slot = new SlotState
                {
                    ID = item.ID,
                    Description = item.Description,
                    Enabled = item.Enabled,
                    NumberSignedUp = item.ApplicantsSignedUp.HasContent() ? item.ApplicantsSignedUp.Count() : 0,
                    TotalPlacesAvailable = item.TotalPlacesAvailable,
                    Status = GenerateSlotStatus(item.ID, user, signup, userSignups),
                    SignupType = GenerateUserSignupType(item, user.ID),
                    Closed = !isAdmin && (signup.Closed || signup.CloseDate < DateTime.Now),
                    OverrideClose = signup.OverrideClose,
                    MergeReserve = signup.MergeReserve,
                    TotalInterestedAvaiable = item.InterestedPlaces,
                    TotalReserveAvailable = item.ReservePlaces
                };


                slotCollection.Add(slot);
            }

            slotView.Description = signup.Description;
            slotView.SlotState = slotCollection;
            slotView.HasSignedUp = signup.Slots.FirstOrDefault(n => n.ApplicantsSignedUp != null && n.ApplicantsSignedUp.Any(u => String.Equals(u.User.Username, _username, StringComparison.CurrentCultureIgnoreCase))) != null;


            return slotView;
        }

        public SlotStatus GenerateSlotStatus(int _slotId, User _user, Signup _signup, IEnumerable<rep.Signup> _signups)
        {
            var slot = _signup.Slots.Single(n => n.ID == _slotId);

            if (!slot.Enabled)
                return SlotStatus.Closed;


            if (_signup.Closed && !_signup.OverrideClose)//checked for overide? - centralise closed logic??
                return SlotStatus.Closed;

            var userSignedUpToSlot = slot.ApplicantsSignedUp.HasContent() && slot.ApplicantsSignedUp.Any(n => String.Equals(n.User.Username, _user.Username, StringComparison.CurrentCultureIgnoreCase));
            var userSignedUpToAnySlot = _signup.Slots.Any(n => n.ApplicantsSignedUp.HasContent() && n.ApplicantsSignedUp.Any(m => String.Equals(m.User.Username, _user.Username, StringComparison.CurrentCultureIgnoreCase)));

            var signedUpToOtherSitesOnDate = _signups.Any(n => n.Date == _signup.Date && n.idSite != Convert.ToInt32(_signup.Location.ID));
            var signedUpToOtherGroupOnDate = _signups.Any(n => n.Date == _signup.Date && n.Group.ID != _signup.Group.ID);
            


            if (userSignedUpToSlot)
                return SlotStatus.AlreadySignedUp;

            if (!_signup.AllowMultipleSignups && userSignedUpToAnySlot)
                return SlotStatus.Clash;

            if (!_signup.Group.AllowMultipleSignups)
            {
                if (_signups.Where(g => g.Group.ID == _signup.Group.ID).Any(n => n.Slots.Any(m => m.UsersSignedUp.Any(o => o.IdUser == _user.ID))))
                {
                    return SlotStatus.Clash;
                }
            }

            if (signedUpToOtherSitesOnDate)
                return SlotStatus.Clash;


            if (signedUpToOtherGroupOnDate)
                return SlotStatus.Clash;


            var applicantsSignedUp = slot.ApplicantsSignedUp.HasContent() ? slot.ApplicantsSignedUp.Count() : 0;

            if (applicantsSignedUp < slot.TotalPlacesAvailable)
            {
                return SlotStatus.Signup;
            }

            if (applicantsSignedUp < slot.TotalPlacesAvailable + slot.ReservePlaces)
            {
                return _signup.MergeReserve ? SlotStatus.Signup : SlotStatus.Reserve;
            }

            if (applicantsSignedUp < slot.TotalPlacesAvailable + slot.ReservePlaces + slot.InterestedPlaces)
                return SlotStatus.Interested;

            if (applicantsSignedUp >= slot.TotalPlacesAvailable + slot.ReservePlaces + slot.InterestedPlaces)
                return SlotStatus.Full;

          //  if (!slot.ApplicantsSignedUp.HasContent())
             //   return SlotStatus.Signup;

            return SlotStatus.Signup;//todo: need ERROR here?

        }

        private static bool IsValidEmail(string email)
        {
            if (String.IsNullOrEmpty(email))
                return false;

            const string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                                   + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                                   + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);


            return regex.IsMatch(email);
        }

        //todo: reindex signup
        public bool SignupToSlot(int _signupID, int _slotId, string _username)
        {
            var signup = Fetch(_signupID);

            var slot = signup.Slots.SingleOrDefault(n => n.ID == _slotId);

            if (slot == null)
            {
                notificationService.AddError(string.Format("The requested slot ({0}) could not be found for signup {1}.", _slotId, _signupID));
                return false;
            }

            //TODO: Check for null here?
            var user = userManager.Fetch(_username);
            var loggedInUser = userManager.Fetch(configurationService.GetLoggedInUsername());
            var isAdmin = loggedInUser.Roles.Any(n => n.Name == "Admin" || n.Name == "Super Admin" || n.Name == "UGC");

            object bodyLock;
            lock (dictionaryLock)
            {

                if (!dictionary.TryGetValue(_slotId, out bodyLock))
                {
                    bodyLock = new object();
                    dictionary[_slotId] = bodyLock;
                }
            }

            if (!isAdmin && signup.Closed)
            {
                notificationService.AddIssue("You cannot sign up to this slot. The sign up is closed.");

                return false;
            }

            if (!isAdmin && (DateTime.Now > signup.CloseDate))
            {
                notificationService.AddIssue(string.Format("You cannot sign up to this slot. The sign up closed on {0}.", signup.CloseDate.ToString("dddd d MMMM yyyy")));

                return false;
            }

            lock (bodyLock)
            {
                var signupDate = DateTime.Now;

                string error;

                if (slot.ApplicantsSignedUp.HasContent())
                {
                    if (slot.ApplicantsSignedUp.Any(n => String.Equals(n.User.Username, user.Username, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        notificationService.AddIssue("You have already signed up to this slot.");
                        return false;
                    }

                    if (slot.ApplicantsSignedUp.Count() >= slot.TotalPlacesAvailable + slot.ReservePlaces + slot.InterestedPlaces) //TODO: look into this logic, what happens if no interested places have been generated??
                    {
                        notificationService.AddIssue("The selected slot is now full.");
                        return false;
                    }
                }

                int id;

                //   bool interestedSlot = false;

                // if (slot.ApplicantsSignedUp.HasContent())
                // interestedSlot = slot.ApplicantsSignedUp.Count() >= slot.TotalPlacesAvailable + slot.ReservePlaces;

                if (signupRepository.SignupToSlot(_slotId, user.ID, signupDate, out id))
                {
                    if (signup.Group.EnableEmails)
                    {

                        //+1 to account for the signup we are currently processing
                        var SCEInterestedSignup = slot.ApplicantsSignedUp.HasContent() && slot.ApplicantsSignedUp.Count() >=
                                             slot.ReservePlaces + slot.TotalPlacesAvailable;
                        // slot.ApplicantsSignedUp.Single(n => n.ID == 0).ID = id;
                        var replacements = new Dictionary<string, string>(4)
                        {
                            {"#forename#", user.Forename},
                            {"#surname#", user.Surname},
                            {"#SignupDate#", signup.Date.ToString("dddd d MMMM yyyy")},
                            {"#SlotDescription#", slot.Description},
                            {"#SignupDescription#", signup.Description},
                            {"#GroupDescription#", signup.Group.Name},
                            {"#username#", user.Username},
                            {"#sitename#", configurationService.SiteName()},
                            {"#siteurl#", configurationService.SiteURL()}
                        };

                        // string key = interestedSlot ? "InterestedSignup" : "UserSessionSignup";


                        var key = string.Empty;

                        var addresses = new Collection<string>();

                        if (IsValidEmail(user.Email))
                            addresses.Add(user.Email);

                        if (user.Roles.Any(n => n.Name == "SCE"))
                        {
                            key = "SCESessionSignup";
                            var scedata = userManager.FetchSCEData(user.ID);

                            if (scedata != null)
                            {
                                replacements.Add("#title#", scedata.Title);
                                if (!string.IsNullOrEmpty(scedata.SecretaryEmail) && IsValidEmail(scedata.SecretaryEmail))
                                {
                                    addresses.Add(scedata.SecretaryEmail);
                                }

                                if (!string.IsNullOrEmpty(scedata.EmailOther) && IsValidEmail(scedata.EmailOther))
                                {
                                    addresses.Add(scedata.EmailOther);
                                }
                            }
                        }
                        /*   if (user.Roles.Any(n => n.Name == "Interviewer"))
                        {
                            if (signup.Group.Name == "Observer")
                            {
                                key = "ObserverSessionSignup";
                            }
                            else
                            {

                                key = reserveSignup ? "ReserveSessionSignup" : "InterviewerSessionSignup";
                            }
                        }*/
                        if (SCEInterestedSignup)
                        {
                            key = "SCEInterestedSignup";
                        }

                        if (addresses.HasContent())
                        {
                            if (emailService.SendMail(key, addresses, replacements))
                            {
                                emailService.SendEmailLog(key, user.Username);
                                return true;
                            }
                        }
                        //else
                        //{
                        //add error?
                        //}
                        return true;
                    }
                    return true;
                }

                notificationService.AddError("An error occured. ");
                return false;

            }


        }
        //todo: reindex signup
        public bool CancelSignupToSlot(int _signupID, int _slotId, string _username)
        {
            var signup = Fetch(_signupID);

            var slot = signup.Slots.SingleOrDefault(n => n.ID == _slotId);

            if (slot == null)
            {
                notificationService.AddError(string.Format("The requested slot ({0}) could not be found for signup {1}.", _slotId, _signupID));
                return false;
            }

            //TODO: check for null here??
            var user = userManager.Fetch(_username);
            // var profile = userManager.FetchProfile(_username);

            object bodyLock;
            lock (dictionaryLock)
            {
                if (!dictionary.TryGetValue(_slotId, out bodyLock))
                {
                    bodyLock = new object();
                    dictionary[_slotId] = bodyLock;
                }
            }

            lock (bodyLock)
            {//todo: NOTE that SCE bumps from interested to reserve. MMI bumps from reserve to main
                var index = slot.ApplicantsSignedUp.FindIndex(n => n.User.ID == user.ID) +1;
                var BumpUser = index <= slot.TotalPlacesAvailable + slot.ReservePlaces && slot.ApplicantsSignedUp.Count() > slot.TotalPlacesAvailable + slot.ReservePlaces;//slot.ApplicantsSignedUp.Count() > slot.TotalPlacesAvailable;// + slot.ReservePlaces;

                if (signupRepository.CancelSignupToSlot(_slotId, user.ID))
                {
                    if (signup.Group.EnableEmails)
                    {
                        var userIndex = slot.ApplicantsSignedUp.FindIndex(n => n.User.ID == user.ID) + 1;

                        var reserveCancel = userIndex > slot.TotalPlacesAvailable;
                        /*                    var userSignup = signup.Slots.SingleOrDefault(n => n.ID == _slotId).ApplicantsSignedUp.SingleOrDefault(n => n.Applicant.Username == _username);

                        signup.Slots.SingleOrDefault(n => n.ID == _slotId).ApplicantsSignedUp.Remove(userSignup);
    */
                        /*if (slot.ApplicantsSignedUp().Count() >= slot.ApplicantsSignedUp + slot.ReservePlaces)
                        {
                        
                        }*/


                        //  string key = "UserSessionCancel";

                        var key = string.Empty;

                        if (user.Roles.Any(n => n.Name == "SCE"))
                            key = "SCESessionCancel";

                        /*        if (user.Roles.Any(n => n.Name == "Interviewer"))
                                {
                                    if (signup.Group.Name == "Observer")
                                        key = "ObserverSessionCancel";
                                    else
                                        key = reserveCancel ? "ReserveSessionCancel" : "InterviewerSessionCancel";
                                }
                                */
                        var replacements = new Dictionary<string, string>(4)
                                            {
                                                {"#forename#", user.Forename},
                                                {"#surname#", user.Surname},
                                                {"#SignupDate#", signup.Date.ToString("dddd d MMMM yyyy")},
                                                {"#SlotDescription#", slot.Description},
                                                {"#username#", user.Username},
                                                {"#SignupDescription#", signup.Description},
                                                {"#GroupDescription#", signup.Group.Name},
                                                {"#sitename#", configurationService.SiteName()},
                                                {"#siteurl#", configurationService.SiteURL()}
                                            };

                        var addresses = new Collection<string>();

                        if (IsValidEmail(user.Email))
                            addresses.Add(user.Email);

                        if (user.Roles.Any(n => n.Name == "SCE"))
                        {
                            key = "SCESessionCancel";
                            var scedata = userManager.FetchSCEData(user.ID);

                            if (scedata != null)
                            {
                                replacements.Add("#title#", scedata.Title);

                                if (!string.IsNullOrEmpty(scedata.SecretaryEmail) && IsValidEmail(scedata.SecretaryEmail))
                                {
                                    addresses.Add(scedata.SecretaryEmail);
                                }

                                if (!string.IsNullOrEmpty(scedata.EmailOther) && IsValidEmail(scedata.EmailOther))
                                {
                                    addresses.Add(scedata.EmailOther);
                                }
                            }
                        }
                        if (addresses.HasContent())
                        {
                            if (emailService.SendMail(key, addresses, replacements))
                            {
                                emailService.SendEmailLog(key, user.Username);
                            }
                        }

                        if (BumpUser)
                        {
                            var users = slot.ApplicantsSignedUp.Select(n => n).OrderBy(n => n.SignupDate).ToArray();

                            var userToBump = users[slot.TotalPlacesAvailable + slot.ReservePlaces/* + 1*/].User;
                            
                            replacements = new Dictionary<string, string>(4)
                                            {
                                                {"#forename#", userToBump.Forename},
                                                {"#surname#", userToBump.Surname},
                                                {"#SignupDate#", signup.Date.ToString("dddd d MMMM yyyy")},
                                                {"#SlotDescription#", slot.Description},
                                                {"#username#", user.Username},
                                                {"#SignupDescription#", signup.Description},
                                                {"#GroupDescription#", signup.Group.Name},
                                                {"#sitename#", configurationService.SiteName()},
                                                {"#siteurl#", configurationService.SiteURL()}
                                            };
                            

                            addresses = new Collection<string>();

                            if (IsValidEmail(userToBump.Email))
                                addresses.Add(userToBump.Email);

                            if (userToBump.Roles.Any(n => n.Name == "SCE"))
                            {
                                var scebumpdata = userManager.FetchSCEData(userToBump.ID);

                                if (scebumpdata != null)
                                {
                                    replacements.Add("#title#", scebumpdata.Title);
                                    if (!string.IsNullOrEmpty(scebumpdata.SecretaryEmail) && IsValidEmail(scebumpdata.SecretaryEmail))
                                    {
                                        addresses.Add(scebumpdata.SecretaryEmail);
                                    }

                                    if (!string.IsNullOrEmpty(scebumpdata.EmailOther) && IsValidEmail(scebumpdata.EmailOther))
                                    {
                                        addresses.Add(scebumpdata.EmailOther);
                                    }
                                }
                            }




                            key = "SlotUpgrade";
                            if (addresses.HasContent())
                            {
                                if (emailService.SendMail(key, addresses, replacements))
                                {
                                    emailService.SendEmailLog(key, userToBump.Username);

                                }
                            }
                        }




                        return true;
                    }



                    notificationService.AddError("An error occured. ");
                    return true;
                }

                return false;
            }
        }

        public int FetchRCPActivityCode(int _signupID)
        {
            return signupRepository.FetchRCPActivityCode(_signupID);
        }

        //cache this!
        private rep.UserSignup FetchuserSignup(int _iduser, IEnumerable<int> _groupIds)
        {
            return signupRepository.FetchUserSignup(_iduser, _groupIds);
        }

        private IEnumerable<rep.UserSignup> FetchuserSignups(int _iduser, IEnumerable<int> _groupIds)
        {
            return signupRepository.FetchUserSignups(_iduser, _groupIds);
        }

        virtual public SlotType GenerateHomeViewSlotStatus(rep.Slot _slot, int _userId)
        {
            //   var userPosition = _slot.UsersSignedUp.ToList().FindIndex(n => n.Type ==)
            // throw new NotImplementedException();

            //    if(_slot)

            var userSignup = _slot.UsersSignedUp.SingleOrDefault(n => n.IdUser == _userId);

            //todo: error check incase userSignup is null??

            return (SlotType)userSignup.Type;
        }


        virtual public SlotType GenerateUserSignupType(Slot _slot, int _userId)
        {
            //   var userPosition = _slot.UsersSignedUp.ToList().FindIndex(n => n.Type ==)
            // throw new NotImplementedException();

            //    if(_slot)

            if (_slot.ApplicantsSignedUp.HasContent())
            {
                var userSignup = _slot.ApplicantsSignedUp.SingleOrDefault(n => n.User.ID == _userId);

                if (userSignup != null)
                {
                    var usersIndex = _slot.ApplicantsSignedUp.FindIndex(n => n.User.ID == _userId) + 1;

                    if (usersIndex <= _slot.TotalPlacesAvailable)
                        return SlotType.Main;

                    if (usersIndex <= _slot.TotalPlacesAvailable + _slot.ReservePlaces)
                        return SlotType.Reserve;

                    return SlotType.Interested;
                }

                //todo: error check incase userSignup is null??
            }

            return SlotType.Interested; //todo: need an error slot?
        }


        #region TESTING STRAIGHT SIGNUP PULLTHROUGH
        public IEnumerable<Models.Objects.SignupsMod.Signup> FetchAllM()
        {
            var repSignups = signupRepository.FetchAll();
            var signups = Mapper.Map<IEnumerable<rep.Signup>, IEnumerable<Models.Objects.SignupsMod.Signup>>(repSignups);

            var users = userManager.Fetch(signups.SelectMany(n => n.Slots).SelectMany(m => m.UsersSignedUp).Select(o => o.IdUser)).ToDictionary(k => k.ID, v => v);
            var locationDict = formManager.FetchFormList("Sites")
    .Collection.ToDictionary(k => k.Value, v => new Location { ID = v.Value, Name = v.Text });
            foreach (var repSignup in repSignups)
            {


                foreach (var signup in signups)
                {
                    if (repSignup.id == signup.Id)
                        signup.Location = locationDict[repSignup.idSite.ToString(CultureInfo.InvariantCulture)];

                    foreach (var slot in signup.Slots)
                    {
                        slot.MergeReserve = signup.MergeReserve;
                        foreach (var user in slot.UsersSignedUp)
                        {
                            user.User = users[user.IdUser];
                            // break;
                        }
                    }
                }
            }

            return signups;
        }

        public Models.Objects.SignupsMod.Signup FetchM(int _id)
        {
            var signup = Mapper.Map<rep.Signup, Models.Objects.SignupsMod.Signup>(signupRepository.Fetch(_id));

            var users = userManager.Fetch(signup.Slots.SelectMany(m => m.UsersSignedUp).Select(o => o.IdUser)).ToDictionary(k => k.ID, v => v);

            foreach (var slot in signup.Slots)
            {
                slot.MergeReserve = signup.MergeReserve;
                foreach (var user in slot.UsersSignedUp)
                {
                    user.User = users[user.IdUser];
                    // break;
                }
            }

            return signup;
        }

        public IEnumerable<Models.Objects.SignupsMod.Signup> FetchM(IEnumerable<int> _ids)
        {
            var repSignups = signupRepository.FetchSignups(_ids);
            var signups = Mapper.Map<IEnumerable<rep.Signup>, IEnumerable<Models.Objects.SignupsMod.Signup>>(repSignups);

            var users = userManager.Fetch(signups.SelectMany(n => n.Slots).SelectMany(m => m.UsersSignedUp).Select(o => o.IdUser)).ToDictionary(k => k.ID, v => v);
            var locationDict = formManager.FetchFormList("Sites")
    .Collection.ToDictionary(k => k.Value, v => new Location { ID = v.Value, Name = v.Text });

            foreach (var repSignup in repSignups)
            {


                foreach (var signup in signups)
                {
                    if (repSignup.id == signup.Id)
                        signup.Location = locationDict[repSignup.idSite.ToString(CultureInfo.InvariantCulture)];

                    foreach (var slot in signup.Slots)
                    {
                        slot.MergeReserve = signup.MergeReserve;
                        foreach (var user in slot.UsersSignedUp)
                        {
                            user.User = users[user.IdUser];
                            // break;
                        }
                    }
                }
            }

            return signups;
        }

     /*   public Models.Objects.SignupsMod.UserSignup FetchUserSignup(int _userId, IEnumerable<int> _groupIds)
        {
            return Mapper.Map<rep.UserSignup, Models.Objects.SignupsMod.UserSignup>(signupRepository.FetchUserSignup(_userId, _groupIds));
        }*/

        public IEnumerable<Models.Objects.SignupsMod.UserSignup> FetchUserSignups(int _userId, IEnumerable<int> _groupIds)
        {
            return Mapper.Map<IEnumerable<rep.UserSignup>, IEnumerable<Models.Objects.SignupsMod.UserSignup>>(signupRepository.FetchUserSignups(_userId, _groupIds));
        }
        #endregion

        public SearchResult DoSearch(BasicSearch _search)
        {
            var newSearch = Mapper.Map<BasicSearch, emSearch>(_search);

            if (_search.Filter.HasContent())
            {
                foreach (var filter in _search.Filter)
                {
                    newSearch.Filters.Add(filter.Key, new emSearch.SearchTerm { Field = filter.Value, Term = Occur.MUST });
                }
            }

            if (!string.IsNullOrEmpty(newSearch.Query))
            {
                newSearch.CustomQuery = new Dictionary<string, emSearch.SearchTerm>
                {
                    {"Username", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Forename", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Surname", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Email", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"SignupDescription", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"SlotDescription", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Group", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                };

            }

            return searchManager.DoSearch(newSearch);
        }

        public void ReindexSearchRecords()
        {
            var records = FetchAllM();

            if (!records.HasContent())
            {
                //todo: send an error message here
                return;
            }

            foreach (var item in records)
            {
                searchManager.Add(new SignupSearchDocument(item));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using eMotive.Managers.Interfaces;
using eMotive.Managers.Objects.Search;
using eMotive.MMI.SignalR;
using eMotive.Models.Objects.Signups;
using eMotive.Search.Interfaces;
using eMotive.Services.Interfaces;
using Microsoft.AspNet.SignalR;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using Signup = eMotive.Models.Objects.SignupsMod.Signup;
using UserSlotView = eMotive.Models.Objects.SignupsMod.UserSlotView;


namespace eMotive.Api
{


  /*  [Route("/Courses/New", "GET")]
    public class NewCourse
    {
    }*/

    [Route("/Sessions")]
    [Route("/Sessions/{Ids}")]
    public class GetSessions
    {
        public int[] Ids { get; set; }
    }

    [Route("/Sessions/Groups")]
    [Route("/Sessions/Groups/{Ids}")]
    public class GetGroups
    {
        public int[] Ids { get; set; }
    }

    [Route("/Sessions/Signup/Add", "POST")]
    public class SlotSignup
    {
        public int IdSignup { get; set; }
        public int IdSlot { get; set; }
        public string Username { get; set; }
    }

    [Route("/Sessions/Save", "POST")]
    public class SaveSignup
    {
        public Signup Signup { get; set; }
    }

    [Route("/Sessions/Signup/Remove", "POST")]
    public class SlotCancel
    {
        public int IdSignup { get; set; }
        public int IdSlot { get; set; }
        public string Username { get; set; }
    }

    [Route("/Sessions/Signups/Emails/StandDown", "POST")]
    public class StandDownExaminers
    {
    }

    [Route("/Sessions/Signup/WillingToChange", "POST")]
    public class WillingToChange : WillingToChangeSignup
    {
    }


    [Authenticate] 
    public class SessionService : Service
    {
        private readonly ISessionManager _sessionManager;
        private readonly ISearchManager _searchManager;

        public SessionService(ISessionManager sessionManager, ISearchManager searchManager)
        {
            _sessionManager = sessionManager;
            _searchManager = searchManager;
        }

        public INotificationService NotificationService { get; set; }
 
        public object Get(GetSessions request)
        {
            var result = request.Ids.IsEmpty()
                ? null
                : _sessionManager.FetchM(request.Ids);

            var success = result != null;

            var issues = NotificationService.FetchIssues(); //TODO: how to deal with errors when going directly into the api?? perhaps organise messages better?

            return new ServiceResult<IEnumerable<Signup>>
            {
                Success = success,
                Result = result,
                Errors = issues
            };
        }

        public object Get(GetGroups request)
        {
            var result = request.Ids.IsEmpty()
                ? _sessionManager.FetchAllGroups()
                : null;//todo: nned a fetch group on ids in sessionManager

            var success = result != null;

            var issues = NotificationService.FetchIssues(); //TODO: how to deal with errors when going directly into the api?? perhaps organise messages better?

            return new ServiceResult<IEnumerable<Models.Objects.Signups.Group>>
            {
                Success = success,
                Result = result,
                Errors = issues
            };
        }

        public object Post(WillingToChange request)
        {
            if (_sessionManager.WillingToChangeSignup(request))
            {
                return new ServiceResult<bool>
                {
                    Success = true,
                    Result = true,
                    Errors = null
                };
            }

            var issues = NotificationService.FetchIssues();

            return new ServiceResult<bool>
            {

                Success = false,
                Result = false,
                Errors = issues
            };
        }

        public object Post(StandDownExaminers request)
        {
            if (_sessionManager.StandDownExaminers())
            {
                return new ServiceResult<bool>
                {
                    Success = true,
                    Result = true,
                    Errors = null
                };
            }

            var issues = NotificationService.FetchIssues();

            return new ServiceResult<bool>
            {

                Success = false,
                Result = false,
                Errors = issues
            };
        }

        public object Post(SaveSignup sessionSignup)
        {
            if (_sessionManager.Save(sessionSignup.Signup))
            {
                _searchManager.Update(new SignupSearchDocument(sessionSignup.Signup));

                return new ServiceResult<bool>
                {
                    Success = true,
                    Result = true,
                    Errors = null
                };
            }

            var issues = NotificationService.FetchIssues();

            return new ServiceResult<bool>
            {

                Success = false,
                Result = false,
                Errors = issues
            };
        }
        
        public object Post(SlotSignup request)
        {
            if (_sessionManager.SignupToSlot(request.IdSignup, request.IdSlot, request.Username))
            {
                var signup = _sessionManager.FetchM(request.IdSignup);
                if (signup != null)
                    _searchManager.Update(new SignupSearchDocument(signup));
              

        //        signup.si
                var slotView = new UserSlotView { Signup = signup, LoggedInUser = request.Username };


                slotView.Initialise(request.Username);
                var slot = slotView.Signup.Slots.Single(n => n.id == request.IdSlot);
             //   slot.GeneratePlacesAvailableString();

                //(int _slotID, string _description, string _badges, string _rowStatus, string _buttonStatus, string _functionality)
                SignupNumbersPush(signup.Id, signup.SlotsAvailableString,signup.SignedUp(request.Username), "warning");
                SlotNumbersPush(request.IdSlot, slot.SlotsAvailableString, slotView.HomeViewRowBadge(request.IdSlot), slotView.HomeViewRowStyle(request.IdSlot), slotView.HomeViewRowButton(request.IdSlot), slotView.AssignStatusFunctionality(request.IdSlot, request.Username), slotView.SlotStatusName(request.IdSlot), request.Username);
                UserSignupConfirm(request.IdSlot, slot.SlotsAvailableString, slotView.HomeViewRowBadge(request.IdSlot), slotView.HomeViewRowStyle(request.IdSlot), slotView.HomeViewRowButton(request.IdSlot), slotView.AssignStatusFunctionality(request.IdSlot, request.Username), slotView.SlotStatusName(request.IdSlot), request.Username);
                
                /*                userSlotView.LoggedInUser = User.Identity.Name ?? string.Empty;
                userSlotView.Signup = slotsM;
                userSlotView.HeaderText = sb.ToString();
                userSlotView.FooterText = pageManager.Fetch("Interview-Date-Page-Footer").Text;


                userSlotView.Initialise(userSlotView.LoggedInUser);*/
                
                
                return new ServiceResult<bool>
                {
                    Success = true,
                    Result = true,
                    Errors = null
                };
            }

            var issues = NotificationService.FetchIssues();

            return new ServiceResult<bool>
            {

                Success = false,
                Result = false,
                Errors = issues
            };
        }

        public object Post(SlotCancel request)
        {
            if (_sessionManager.CancelSignupToSlot(request.IdSignup, request.IdSlot, request.Username))
            {
                var signup = _sessionManager.FetchM(request.IdSignup);
                if (signup != null)
                    _searchManager.Delete(new SignupSearchDocument(signup));

                signup.GenerateSlotsAvailableString();
                SignupNumbersPush(signup.Id, signup.SlotsAvailableString,signup.SignedUp(request.Username), "warning");

                var slotView = new UserSlotView { Signup = signup, LoggedInUser = request.Username}; 
                slotView.Initialise(request.Username);
                var slot = slotView.Signup.Slots.Single(n => n.id == request.IdSlot);


                
                
              //  slot.GeneratePlacesAvailableString();
                //SlotNumbersPush(request.IdSlot, slot.SlotsAvailableString, "warning");//do we need to do a GenerateString thing here???
                SlotNumbersPush(request.IdSlot, slot.SlotsAvailableString, slotView.HomeViewRowBadge(request.IdSlot), slotView.HomeViewRowStyle(request.IdSlot), slotView.HomeViewRowButton(request.IdSlot), slotView.AssignStatusFunctionality(request.IdSlot, request.Username), slotView.SlotStatusName(request.IdSlot), request.Username);
                UserSignupConfirm(request.IdSlot, slot.SlotsAvailableString, slotView.HomeViewRowBadge(request.IdSlot), slotView.HomeViewRowStyle(request.IdSlot), slotView.HomeViewRowButton(request.IdSlot), slotView.AssignStatusFunctionality(request.IdSlot, request.Username), slotView.SlotStatusName(request.IdSlot), request.Username);
                
                return new ServiceResult<bool>
                {
                    Success = true,
                    Result = true,
                    Errors = null
                };
            }

            //var issues = NotificationService.FetchIssues();

            return new ServiceResult<bool>
            {

                Success = false,
                Result = false,
                Errors = new[] {"An error occurred. The signup could not be cancelled."}
            };
        }

        private void UserSignupConfirm(int _slotID, string _description, string _badges, string _rowStatus, string _buttonStatus, string _functionality, string _buttonText, string _username)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MMIHub>();

            hubContext.Clients.User(_username).signupConfirm(new
            {
                SlotId = _slotID,
                Description = _description,
                Badges = _badges,
                RowStatus = _rowStatus,
                ButtonStatus = _buttonStatus,
                Functionality = _functionality,
                ButtonText = _buttonText,
                Username = _username
            });

            /*
            hubContext.Clients.Group("SignupConfirm")*/
        }

        private void SignupNumbersPush(int _signupID, string _placesRemaining, bool _isSignedUp, string _status)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MMIHub>();

            hubContext.Clients.Group("SignupSelection").placesChanged(new
            {
                SignUpId = _signupID,
                PlacesRemaining = _placesRemaining,
                SignedUp = _isSignedUp,
                Status = _status
            });
        }

        private void SlotNumbersPush(int _slotID, string _description, string _badges, string _rowStatus, string _buttonStatus, string _functionality, string _buttonText, string _username)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MMIHub>();
            
           // var test = hubContext.Clients.User("dsfds").;
            var user = MMIHub.GetUser(_username);

            var except = new [] {string.Empty};

            if (user != null)
                except = user.ConnectionIds.ToArray();

            hubContext.Clients.Group("SlotSelection", except).placesChanged(new
            {
                SlotId = _slotID,
                Description = _description,
                Badges = _badges,
                RowStatus = _rowStatus,
                ButtonStatus = _buttonStatus,
                Functionality = _functionality,
                ButtonText = _buttonText,
                Username = _username
            });
        }

    }
}

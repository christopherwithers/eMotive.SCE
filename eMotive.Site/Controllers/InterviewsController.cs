using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using eMotive.Managers.Interfaces;
using eMotive.Services.Interfaces;
using ServiceStack.Mvc;

namespace eMotive.SCE.Controllers
{
    [Common.ActionFilters.Authorize(Roles = "SCE")]
    public class InterviewsController : ServiceStackController
    {
        private readonly ISessionManager signupManager;
        private readonly IPartialPageManager pageManager;
        private readonly IUserManager userManager;

        public InterviewsController(ISessionManager _signupManager, IPartialPageManager _pageManager, IUserManager _userManager)
        {
            signupManager = _signupManager;
            pageManager = _pageManager;
            userManager = _userManager;
        }

        //[Inject]
        public IeMotiveConfigurationService ConfigurationService { get; set; }
       // [Inject]
        public INotificationService NotificationService { get; set; }

       /* public ActionResult Disability()
        {
            var signups = signupManager.FetchSignupInformation(User.Identity.Name);

            var pageText = pageManager.FetchPartials(new[] { "Disability-Session-List-header", "Disability-Session-List-Footer" }).ToDictionary(k => k.Key, v => v.Text);
            signups.HeaderText = pageText["Disability-Session-List-header"];
            signups.FooterText = pageText["Disability-Session-List-Footer"];

       //     signups.HeaderText = pageManager.Fetch("Disability-Session-List-header").Text;
            
            return View(signups);
        }*/

        public ActionResult Test()
        {
            return View();
        }

        public ActionResult Signups()
        {
            var signups = signupManager.FetchSignupInformation(User.Identity.Name);


            Dictionary<string, string> pageText = null;

            pageText = pageManager.FetchPartials(new[] { "SCE-Session-List-Header", "SCE-Session-List-Footer" }).ToDictionary(k => k.Key, v => v.Text);


            signups.HeaderText = pageText["SCE-Session-List-Header"];
            signups.FooterText = pageText["SCE-Session-List-Footer"];

            signups.GroupDictionary = signupManager.FetchGroups(signups.SignupInformation.Select(n => n.Group.ID)).ToDictionary(k => k.Name,  v=> v);
            signups.LoggedInUser = userManager.Fetch(User.Identity.Name);
            signups.WillingToChange = signupManager.FetchWillingToChangeForUser(signups.LoggedInUser.ID);

            return View(signups);
        }

        public ActionResult Withdraw()
        {
            return View();
        }

        public ActionResult TestPage()
        {
            var signups = signupManager.FetchSignupInformation(User.Identity.Name);
            var user = userManager.Fetch(User.Identity.Name);

            Dictionary<string, string> pageText = null;

            if (user.Roles.Any(n => n.Name == "SCE"))
            {
                pageText = pageManager.FetchPartials(new[] {"Session-List-header", "Session-List-Footer"}).ToDictionary(k => k.Key, v => v.Text);
                signups.HeaderText = pageText["Session-List-header"];
                signups.FooterText = pageText["Session-List-Footer"];
            }
            else
            {
                pageText = pageManager.FetchPartials(new[] { "Session-List-header", "Session-List-Footer" }).ToDictionary(k => k.Key, v => v.Text);
                signups.HeaderText = pageText["Session-List-header"];
                signups.FooterText = pageText["Session-List-Footer"];
            }

            return View(signups);
        }

        public ActionResult Slots(int? id)
        {
            var slots = signupManager.FetchSlotInformation(id.HasValue ? id.Value : -1, User.Identity.Name);
       //     var user = userManager.Fetch(User.Identity.Name);

            Dictionary<string, string> pageText;


                pageText = pageManager.FetchPartials(new[] { "SCE-Interview-Date-Page", "SCE-Interview-Date-Page-Footer" }).ToDictionary(k => k.Key, v => v.Text);


            if (slots != null)
            {
                var replacements = new Dictionary<string, string>(4)
                {
                    {"#signupdate#", slots.Date.ToString("dddd d MMMM yyyy")},
                    {"#signupdescription#", slots.Description},
                    {"#groupdescription#", slots.Group.Name}
                };

                var sbHead = new StringBuilder(pageText["SCE-Interview-Date-Page"]);
                var sbFoot = new StringBuilder(pageText["SCE-Interview-Date-Page-Footer"]);

                foreach (var replacment in replacements)
                {
                    sbHead.Replace(replacment.Key, replacment.Value);
                    sbFoot.Replace(replacment.Key, replacment.Value);
                }

                slots.HeaderText = sbHead.ToString();
                slots.FooterText = sbFoot.ToString();

                slots.LoggedInUser = User.Identity.Name ?? string.Empty;
            }

            return View(slots);
        }

       /* [AjaxOnly]
        public CustomJsonResult SignupToSlot(int idSignup, int idSlot)
        {
            if (signupManager.SignupToSlot(idSignup, idSlot, User.Identity.Name))
            {
                var signup = signupManager.Fetch(idSignup);
                var slot = signup.Slots.Single(n => n.ID == idSlot);

                ApplicantSignupPush(signup.ID, signup.Slots.Sum(n => n.TotalPlacesAvailable),
                    signup.Slots.Sum(n => n.ApplicantsSignedUp.HasContent() ? n.TotalPlacesAvailable - n.ApplicantsSignedUp.Count() : n.TotalPlacesAvailable));

                ApplicantSlotPush(slot.ID, slot.TotalPlacesAvailable,
                    slot.ApplicantsSignedUp.HasContent() ? slot.TotalPlacesAvailable - slot.ApplicantsSignedUp.Count() : slot.TotalPlacesAvailable);

                return new CustomJsonResult
                    {
                        Data = new {success = true, message = "successfully signed up."}
                    };
            }

            var issues = NotificationService.FetchIssues();


                return new CustomJsonResult
                {
                    Data = new { success = false, message = issues }
                };
            
        }

        [AjaxOnly]
        public CustomJsonResult CancelSignupToSlot(int idSignup, int idSlot)
        {
            if (signupManager.CancelSignupToSlot(idSignup, idSlot, User.Identity.Name))
            {
                var signup = signupManager.Fetch(idSignup);
                var slot = signup.Slots.Single(n => n.ID == idSlot);

                ApplicantSignupPush(signup.ID, signup.Slots.Sum(n => n.TotalPlacesAvailable),
                    signup.Slots.Sum(n => n.ApplicantsSignedUp.HasContent() ? n.TotalPlacesAvailable - n.ApplicantsSignedUp.Count() : n.TotalPlacesAvailable));

                ApplicantSlotPush(slot.ID, slot.TotalPlacesAvailable,
                    slot.ApplicantsSignedUp.HasContent() ? slot.TotalPlacesAvailable - slot.ApplicantsSignedUp.Count() : slot.TotalPlacesAvailable);

                return new CustomJsonResult
                {
                    Data = new { success = true, message = "successfully cancelled appointment." }
                };
            }

            return new CustomJsonResult
            {
                Data = new { success = false, message = "An error occurred. The signup could not be cancelled." }
            };
        }*/
        /*
        private void ApplicantSignupPush(int _signupID, int _totalPlaces, int _remainingPlaces)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MMIHub>();

            hubContext.Clients.Group("SignupSelection").placesChanged(new
            {
                SignUpId = _signupID,
                TotalPlaces = _totalPlaces,
                PlacesAvailable = _remainingPlaces
            });
        }

        private void ApplicantSlotPush(int _slotID, int _totalPlaces, int _remainingPlaces)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MMIHub>();

            hubContext.Clients.Group("SignupSelection").slotChanged(new
            {
                SlotId = _slotID,
                TotalPlaces = _totalPlaces,
                PlacesAvailable = _remainingPlaces
            });
        }*/

    }
}

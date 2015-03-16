using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.SignupsMod;
using eMotive.Services.Interfaces;
//using Ninject;
using ServiceStack.Mvc;
using UserSlotView = eMotive.Models.Objects.Signups.UserSlotView;

namespace eMotive.SCE.Controllers
{
    public class SessionsController : ServiceStackController
    {
        private readonly ISessionManager signupManager;
        private readonly IPartialPageManager pageManager;

        public SessionsController(ISessionManager _signupManager, IPartialPageManager _pageManager)
        {
            signupManager = _signupManager;
            pageManager = _pageManager;
        }

       // [Inject]
        public IeMotiveConfigurationService ConfigurationService { get; set; }
       // [Inject]
        public INotificationService NotificationService { get; set; }

        public ActionResult Index(int id)
        {
            var signup = signupManager.FetchSignupInformation(User.Identity.Name, id);

            var pageText = pageManager.FetchPartials(new[] { "Session-List-header", "Session-List-Footer" }).ToDictionary(k => k.Key, v => v.Text);
            signup.HeaderText = pageText["Session-List-header"];
            signup.FooterText = pageText["Session-List-Footer"];

            return View(signup);
        }

        public ActionResult Signups()
        {

           /* var pageText = pageManager.FetchPartials(new[] { "Session-List-header", "Session-List-Footer" }).ToDictionary(k => k.Key, v => v.Text);
            signups.HeaderText = pageText["Session-List-header"];
            signups.FooterText = pageText["Session-List-Footer"];*/
            var pageText = pageManager.FetchPartials(new[] { "Session-List-header", "Session-List-Footer" }).ToDictionary(k => k.Key, v => v.Text);

            var userSignup = new UserSignupView
            {
                LoggedInUser = User.Identity.Name ?? string.Empty,
                Signups = signupManager.FetchAllM(),
                HeaderText = pageText["Session-List-header"] ?? string.Empty,
                FooterText = pageText["Session-List-Footer"] ?? string.Empty
            };

            userSignup.Initialise(User.Identity.Name); // Perhaps a bit messy, but will think how to tidy this up! Pre-optimising?

            return View(userSignup);
        }

        public ActionResult Slots(int? id)
        {
           // var slots = signupManager.FetchSlotInformation(id.HasValue ? id.Value : -1, User.Identity.Name);
            var slotsM = signupManager.FetchM(id.Value);
            var userSlotView = new Models.Objects.SignupsMod.UserSlotView();
            if (slotsM.Slots != null)
            {
                var replacements = new Dictionary<string, string>(4)
                {
                    {"#interviewdate#", slotsM.Date.ToString("dddd d MMMM yyyy")},
                    {"#description#", slotsM.Description},
                    {"#group#", slotsM.Group.Name}
                };
                //Disability-Interview-Date-Page
                var sb = new StringBuilder(pageManager.Fetch("Interview-Date-Page").Text);

                foreach (var replacment in replacements)
                {
                    sb.Replace(replacment.Key, replacment.Value);
                }
                userSlotView.LoggedInUser = User.Identity.Name ?? string.Empty;
                userSlotView.Signup = slotsM;
                userSlotView.HeaderText = sb.ToString();
                userSlotView.FooterText = pageManager.Fetch("Interview-Date-Page-Footer").Text;


                userSlotView.Initialise(userSlotView.LoggedInUser);
            }

            return View(userSlotView);
        }

    }
}

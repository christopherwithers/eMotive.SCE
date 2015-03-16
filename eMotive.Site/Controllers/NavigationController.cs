using System;
using System.Linq;
using System.Web.Mvc;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Menu;
using eMotive.Models.Objects.Users;
using eMotive.Services.Interfaces;
using ServiceStack.Mvc;

namespace eMotive.SCE.Controllers
{
    public class NavigationController : ServiceStackController
    {
       // private readonly INavigationServices navigationService;
        private readonly IUserManager userManager;
        private readonly IeMotiveConfigurationService config;

        private User user;


        public NavigationController(IUserManager _userManager, IeMotiveConfigurationService _config)
        {
            userManager = _userManager;
            config = _config;

        }

        public string UserWelcome()
        {
            if (user == null)
                return "<p>Welcome <b>";
            user = userManager.Fetch(User.Identity.Name);
            return string.Concat("<p>Welcome <b>", user.Forename, " ", user.Surname, "</b></p><p>", DateTime.Now.ToString("dddd d MMMM yyyy"), "</p>");
        }

        public PartialViewResult MainMenu()
        {
            return PartialView("_mainMenu", User.Identity.IsAuthenticated ? FetchMainMenu(true) : FetchMainMenu(false));
        }

        private Menu FetchMainMenu(bool _loggedIn)
        {

            if (!_loggedIn)
                return BuildLoggedOutMenu();

            if (user == null)
                user = userManager.Fetch(User.Identity.Name);

           // if (user.Roles.Any(n => n.Name == "SCE"))
          //  {
          //      return BuildSCEMenu();
           // }
            //applicant menu
            return BuildSCEMenu(config.AllowWithdrawals());
        }

        private Menu BuildLoggedOutMenu()
        {
            var menu = new Menu
            {
                ID = 1,
                Title = "LoggedOutMenu",
                MenuItems = new[]
                            {
                                 new MenuItem
                                    {
                                        ID = 1,
                                        Name = config.SiteName(),
                                        URL = Url.Action("Login", "Account"),//"/SCE/Account/Login",
                                        Title = string.Format("{0} Public Homepage", config.SiteName()),
                                    }
                            }
            };

            return menu;
        }

        private Menu BuildInterviewerMenu()
        {
            var menu = new Menu
            {
                ID = 1,
                Title = "Interviewer Menu",
                MenuItems = new[]
                            {
                                 new MenuItem
                                    {
                                        ID = 1,
                                        Name = string.Format("{0} Home", config.SiteName()),
                                        URL = Url.Action("Index","Home"),//"/SCE/Home/",
                                        Title = string.Format("{0} Home", config.SiteName()),
                                    },
                                 new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Sessions",
                                        URL = Url.Action("Signups","Interviews"),//"/SCE/Interviews/Signups",
                                        Title = "View Session Slots"
                                    },
                                    new MenuItem
                                    {
                                        ID = 2,
                                        Name = "My Details",
                                        URL = Url.Action("InterviewerDetails","Account"),//"/SCE/Account/Details",
                                        Title = "My Details"
                                    },
                                 new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Change Password",
                                        URL = Url.Action("Details","Account"),//"/SCE/Account/Details",
                                        Title = "Change Password"
                                    }, 
                                 new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Contact Us",
                                        URL = Url.Action("ContactUs","Home"),//"/SCE/Home/ContactUs",
                                        Title = "Our Contact Details"
                                    },
                                 new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Logout",
                                        URL = Url.Action("Logout","Account"),//"/SCE/Account/Logout",
                                        Title = "Logout"
                                    }

                            }
            };

            return menu;
        }

        private Menu BuildSCEMenu(bool allowWithdrawal)
        {
            var menu = new Menu
                {
                    ID = 1,
                    Title = "SCEMenu",
                    MenuItems = new[]
                    {
                        new MenuItem
                        {
                            ID = 1,
                            Name = string.Format("{0} Home", config.SiteName()),
                            URL = Url.Action("Index", "Home"), //"/SCE/Home/",
                            Title = string.Format("{0} Home", config.SiteName()),
                        },
                        new MenuItem
                        {
                            ID = 2,
                            Name = "Sessions",
                            URL = Url.Action("Signups", "Interviews"), //"/SCE/Interviews/Signups",
                            Title = "View Session Slots"
                        },
                        new MenuItem
                                    {
                                        ID = 2,
                                        Name = "My Details",
                                        URL = Url.Action("InterviewerDetails","Account"),//"/SCE/Account/Details",
                                        Title = "My Details"
                                    },
                        new MenuItem
                        {
                            ID = 2,
                            Name = "Change Password",
                            URL = Url.Action("Details", "Account"), //"/SCE/Account/Details",
                            Title = "Change Password"
                        },
                        new MenuItem
                        {
                            ID = 2,
                            Name = "Contact Us",
                            URL = Url.Action("ContactUs", "Home"), //"/SCE/Home/ContactUs",
                            Title = "Our Contact Details"
                        },
                        new MenuItem
                        {
                            ID = 2,
                            Name = "Logout",
                            URL = Url.Action("Logout", "Account"), //"/SCE/Account/Logout",
                            Title = "Logout"
                        }

                    }
                };
            
            return menu;
        }

        [OutputCache(Duration = 10, VaryByParam = "none")]
        public ActionResult MetaTags()
        {
            var tags = config.MetaTags();

            return Content(string.Format("<meta name=\"keywords\" content=\"{0}\">", tags));
        }

        [OutputCache(Duration = 10, VaryByParam = "none")]
        public ActionResult GoogleAnalyticsObject()
        {
            var code = config.GoogleAnalytics();

            return Content(string.Format("<script>{0}</script>", code));
        }

    }
}

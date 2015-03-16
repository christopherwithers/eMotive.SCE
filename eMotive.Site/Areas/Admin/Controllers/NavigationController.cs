using System.Linq;
using System.Web.Mvc;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Menu;
using eMotive.Services.Interfaces;
using ServiceStack.Mvc;

namespace eMotive.SCE.Areas.Admin.Controllers
{//keep this in mind: http://www.verious.com/qa/performance-bottleneck-url-action-can-i-workaround-it/ poss last answer as solution?
    public class NavigationController : ServiceStackController
    {
        private readonly IUserManager userManager;
        private readonly IeMotiveConfigurationService config;

        public NavigationController(IUserManager _userManager, IeMotiveConfigurationService _config)
        {
            userManager = _userManager;
            config = _config;
        }


        public PartialViewResult Menu()
        {

            ViewBag.SiteName = config.SiteName();

            if (!User.Identity.IsAuthenticated)
                return PartialView("_AdminNav", null);

            var user = userManager.Fetch(User.Identity.Name);

            if (user == null)
                return PartialView("_AdminNav", null);

            if (user.Roles.Any(n => n.Name == "Super Admin"))
                return PartialView("_AdminNav", SuperAdminMenu());

            if (user.Roles.Any(n => n.Name == "Admin"))
                return PartialView("_AdminNav", AdminMenu());

            if (user.Roles.Any(n => n.Name == "UGC"))
                return PartialView("_AdminNav", ModeratorMenu());

            return PartialView("_AdminNav", null);
        }

        private Menu SuperAdminMenu()
        {
            return new Menu
            {
                ID = 1,
                Title = "Administration Menu",
                MenuItems = new[]
                        {
                           /* new MenuItem
                                {
                                    ID = 1,
                                    Name = "Home",
                                    URL = Url.Action("Index", "Home", new {area="Admin"}),//"/MMIAdmin/Admin/Home",
                                    Title = "Administration Homepage",
                                    Icon = "<span class='icon-home'></span>"
                                },*/
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Users",
                                    URL = "#",//"/MMIAdmin/Admin/Users",
                                    Title = "User Administration",
                                    Icon = "<span class='icon-user'></span>",
                                    MenuItems = new []
                                        {
                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "List All",
                                                URL = Url.Action("Index", "Users", new {area="Admin"}),//"/MMIAdmin/Admin/Users",
                                                Title = "User Administration",
                                                Icon = "<span class='icon-user'></span>",
                                            },
                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "Roles",
                                                URL = Url.Action("Index", "Roles", new {area="Admin"}),//"/MMIAdmin/Admin/Roles",
                                                Title = "Role Administration",
                                                Icon = "<span class='icon-lock'></span>"
                                            }
                                        }
                                },
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Emails",
                                    URL = Url.Action("Index", "Email", new {area="Admin"}),//"/MMIAdmin/Admin/Email/",
                                    Title = "Email Administration",
                                    Icon = "<span class='icon-envelope'></span>"
                                },
                              /*  new MenuItem
                                {
                                    ID = 1,
                                    Name = "News",
                                    URL = Url.Action("Index", "News", new {area="Admin"}),//"/MMIAdmin/Admin/News/",
                                    Title = "News Administration",
                                    Icon = "<span class='icon-list-alt'></span>"
                                },*/
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Pages",
                                    URL = "#",//"/MMIAdmin/Admin/Pages/",
                                    Title = "Page Administration",
                                    Icon = "<span class='icon-book'></span>",
                                    MenuItems = new []
                                        {                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "Pages",
                                                URL = Url.Action("Index", "Pages", new {area="Admin"}),//"/MMIAdmin/Admin/Users",
                                                Title = "Page Administration",
                                                Icon = "<span class='icon-book'></span>",
                                            },
                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "Forms",
                                                URL = Url.Action("Index", "Forms", new {area="Admin"}),//"/MMIAdmin/Admin/Users",
                                                Title = "Form Administration",
                                                Icon = "<span class='icon-file'></span>",
                                            }
                                        }
                                },
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Signups",
                                    URL = "#",
                                    Title = "Signup Administration",
                                    Icon = "<span class='icon-ok-circle'></span>",
                                    MenuItems = new []
                                        {
                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "List",
                                                URL = Url.Action("Index", "Signups", new {area="Admin"}),//"/MMIAdmin/Admin/Pages/",
                                                Title = "Signup Administration",
                                                Icon = "<span class='icon-ok-circle'></span>",
                                            }, 
                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "Groups",
                                                URL = Url.Action("Groups", "Signups", new {area="Admin"}),//"/MMIAdmin/Admin/Pages/",
                                                Title = "Group Administration",
                                                Icon = "<span class='icon-ok-circle'></span>",
                                            },
                                       /*     new MenuItem
                                            {
                                                ID = 1,
                                                Name = "Open / Close",
                                                URL = Url.Action("OpenClose", "Signups", new {area="Admin"}),//"/MMIAdmin/Admin/Roles",
                                                Title = "Open / Close View",
                                                Icon = "<span class='icon-repeat'></span>",

                                            },*/

                                        }
                                },
                                new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Password",
                                        URL = Url.Action("Details","Account"),//"/SCE/Account/Details",
                                        Title = "Change Password",
                                        Icon = "<span class='icon-cog'></span>"
                                    }, 
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Reports",
                                    URL = Url.Action("Index", "Reports", new {area="Admin"}),//"/MMIAdmin/Admin/Settings/",
                                    Title = "Reports",
                                    Icon = "<span class='icon-file'></span>"
                                },
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Settings",
                                    URL = Url.Action("Index", "Settings", new {area="Admin"}),//"/MMIAdmin/Admin/Settings/",
                                    Title = "eMotive Settings",
                                    Icon = "<span class='icon-cog'></span>"
                                }
                        }
            };
        }

        private Menu AdminMenu()
        {
            return new Menu
            {
                ID = 1,
                Title = "Administration Menu",
                MenuItems = new[]
                        {
                            /*new MenuItem
                                {
                                    ID = 1,
                                    Name = "Home",
                                    URL = Url.Action("Index", "Home", new {area="Admin"}),//"Admin/Home",
                                    Title = "Administration Homepage",
                                    Icon = "<span class='icon-home'></span>"
                                },*/
                            new MenuItem
                                {
                                    ID = 1,
                                    Name = "Users",
                                    URL = "#",//"Admin/Users",
                                    Title = "User Administration",
                                    Icon = "<span class='icon-user'></span>",
                                    MenuItems = new []
                                    {
                                        new MenuItem
                                        {
                                            ID = 1,
                                            Name = "List All",
                                            URL = Url.Action("Index", "Users", new {area="Admin"}),//"Admin/Users",
                                            Title = "User Administration",
                                            Icon = "<span class='icon-user'></span>"
                                        }
                                    }
                                },
                            new MenuItem
                                {
                                    ID = 1,
                                    Name = "Emails",
                                    URL = Url.Action("Index", "Email", new {area="Admin"}),//"Admin/Email/",
                                    Title = "Email Administration",
                                    Icon = "<span class='icon-envelope'></span>"
                                },
                             /*   new MenuItem
                                {
                                    ID = 1,
                                    Name = "News",
                                    URL = Url.Action("Index", "News", new {area="Admin"}),//"Admin/News/",
                                    Title = "News Administration",
                                    Icon = "<span class='icon-list-alt'></span>"
                                },*/
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Pages",
                                    URL = "#",//"/MMIAdmin/Admin/Pages/",
                                    Title = "Page Administration",
                                    Icon = "<span class='icon-book'></span>",
                                    MenuItems = new []
                                        {                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "Pages",
                                                URL = Url.Action("Index", "Pages", new {area="Admin"}),//"/MMIAdmin/Admin/Users",
                                                Title = "Page Administration",
                                                Icon = "<span class='icon-book'></span>",
                                            },
                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "Forms",
                                                URL = Url.Action("Index", "Forms", new {area="Admin"}),//"/MMIAdmin/Admin/Users",
                                                Title = "Form Administration",
                                                Icon = "<span class='icon-file'></span>",
                                            }
                                        }
                                },
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Signups",
                                    URL = "#",
                                    Title = "Signup Administration",
                                    Icon = "<span class='icon-ok-circle'></span>",
                                    MenuItems = new []
                                        {
                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "List",
                                                URL = Url.Action("Index", "Signups", new {area="Admin"}),//"/MMIAdmin/Admin/Pages/",
                                                Title = "Signup Administration",
                                                Icon = "<span class='icon-ok-circle'></span>",
                                            }, 
                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "Groups",
                                                URL = Url.Action("Groups", "Signups", new {area="Admin"}),//"/MMIAdmin/Admin/Pages/",
                                                Title = "Group Administration",
                                                Icon = "<span class='icon-ok-circle'></span>",
                                            }
                                        }
                                },
                                new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Password",
                                        URL = Url.Action("Details","Account"),//"/SCE/Account/Details",
                                        Title = "Change Password",
                                        Icon = "<span class='icon-cog'></span>"
                                    }, 
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Reports",
                                    URL = Url.Action("Index", "Reports", new {area="Admin"}),//"/MMIAdmin/Admin/Settings/",
                                    Title = "Reports",
                                    Icon = "<span class='icon-file'></span>"
                                },
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Settings",
                                    URL = Url.Action("Index", "Settings", new {area="Admin"}),//"/MMIAdmin/Admin/Settings/",
                                    Title = "eMotive Settings",
                                    Icon = "<span class='icon-cog'></span>"
                                }
                        }
            };
        }

        private Menu ModeratorMenu()
        {
            return new Menu
            {
                ID = 1,
                Title = "UGC Menu",
                MenuItems = new[]
                        {
                            /*new MenuItem
                                {
                                    ID = 1,
                                    Name = "Home",
                                    URL = Url.Action("Index", "Home", new {area="Admin"}),//"Admin/Home",
                                    Title = "Administration Homepage",
                                    Icon = "<span class='icon-home'></span>"
                                },*/
                            new MenuItem
                                {
                                    ID = 1,
                                    Name = "Users",
                                    URL = "#",//"Admin/Users",
                                    Title = "User Administration",
                                    Icon = "<span class='icon-user'></span>",
                                    MenuItems = new []
                                    {
                                        new MenuItem
                                        {
                                            ID = 1,
                                            Name = "List All",
                                            URL = Url.Action("Index", "Users", new {area="Admin"}),//"Admin/Users",
                                            Title = "User Administration",
                                            Icon = "<span class='icon-user'></span>"
                                        }
                                    }
                                },
                                new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Password",
                                        URL = Url.Action("Details","Account"),//"/SCE/Account/Details",
                                        Title = "Change Password",
                                        Icon = "<span class='icon-cog'></span>"
                                    }, 
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Reports",
                                    URL = Url.Action("Index", "Reports", new {area="Admin"}),//"/MMIAdmin/Admin/Settings/",
                                    Title = "Reports",
                                    Icon = "<span class='icon-file'></span>"
                                }/*,
                            new MenuItem
                                {
                                    ID = 1,
                                    Name = "Emails",
                                    URL = Url.Action("Index", "Email", new {area="Admin"}),//"Admin/Email/",
                                    Title = "Email Administration",
                                    Icon = "<span class='icon-envelope'></span>"
                                },*/
                             /*   new MenuItem
                                {
                                    ID = 1,
                                    Name = "News",
                                    URL = Url.Action("Index", "News", new {area="Admin"}),//"Admin/News/",
                                    Title = "News Administration",
                                    Icon = "<span class='icon-list-alt'></span>"
                                },*/
                              /*  new MenuItem
                                {
                                    ID = 1,
                                    Name = "Pages",
                                    URL = Url.Action("Index", "Pages", new {area="Admin"}),//"Admin/Pages/",
                                    Title = "Page Administration",
                                    Icon = "<span class='icon-book'></span>"
                                }*//*,
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Signups",
                                    URL = "#",
                                    Title = "Signup Administration",
                                    Icon = "<span class='icon-ok-circle'></span>",
                                    MenuItems = new []
                                        {
                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "List",
                                                URL = Url.Action("Index", "Signups", new {area="Admin"}),//"/MMIAdmin/Admin/Pages/",
                                                Title = "Signup Administration",
                                                Icon = "<span class='icon-ok-circle'></span>",
                                            }, 
                                            new MenuItem
                                            {
                                                ID = 1,
                                                Name = "Groups",
                                                URL = Url.Action("Groups", "Signups", new {area="Admin"}),//"/MMIAdmin/Admin/Pages/",
                                                Title = "Group Administration",
                                                Icon = "<span class='icon-ok-circle'></span>",
                                            }
                                        }
                                }*//*,
                                new MenuItem
                                {
                                    ID = 1,
                                    Name = "Reports",
                                    URL = Url.Action("Index", "Reports", new {area="Admin"}),//"/MMIAdmin/Admin/Settings/",
                                    Title = "Reports",
                                    Icon = "<span class='icon-file'></span>"
                                },*/
                        }
            };
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

//using Microsoft.AspNet.Identity;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using eMotive.Managers.Interfaces;
using eMotive.SCE.Common;
using eMotive.SCE.Common.ActionFilters;
using eMotive.Models.Objects.Account;
using eMotive.Models.Objects.StatusPages;
using eMotive.Models.Objects.Users;
using eMotive.Services.Interfaces;
using Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using ServiceStack;
using ServiceStack.Mvc;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;

namespace eMotive.SCE.Controllers
{
    public class AccountController : ServiceStackController
    {
        private readonly IAccountManager accountManager;
        private readonly IEmailService emailService;
        private readonly IUserManager userManager;
        private readonly IGroupManager groupManager;
        private readonly IPartialPageManager pageManager;

        public AccountController(IAccountManager _accountManager, IUserManager _userManager, IGroupManager _groupManager, IEmailService _emailService, IPartialPageManager _pageManager)
        {
            accountManager = _accountManager;
            userManager = _userManager;
            emailService = _emailService;
            groupManager = _groupManager;
            pageManager = _pageManager;
        }

        IAuthenticationManager Authentication
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

      //  [Inject]
        public INotificationService notificationService { get; set; }

        [HttpGet]
        public ActionResult Login()
        {
            return View(new Login());
        }

        [System.Web.Mvc.Authorize(Roles = "SCE")]
        [HttpGet]
        public ActionResult Withdraw()
        {
            var withdraw = new Withdraw();
            
            var pageText = pageManager.FetchPartials(new[] { "Withdraw-header" }).ToDictionary(k => k.Key, v => v.Text);

            withdraw.PageText = pageText["Withdraw-header"];

            return View(withdraw);
        }

        [HttpPost]
        public ActionResult Withdraw(Withdraw withdraw)
        {
            var pageText = pageManager.FetchPartials(new[] { "Withdraw-header", "Withdrawn-header" }).ToDictionary(k => k.Key, v => v.Text);

            if (withdraw.WithdrawalConfirmation)
            {
                var user = userManager.Fetch(User.Identity.Name);

                if (user != null)
                {
                    if (accountManager.WithdrawUser(user.ID))
                    {
                        withdraw.PageText = pageText["Withdrawn-header"];

                        //log out user
                        Authentication.SignOut();
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        Response.Cache.SetNoStore();


                        return View(withdraw);
                    }
                }                
            }

            withdraw.PageText = pageText["Withdraw-header"]; ;

            return View(withdraw);
        }


        [HttpPost]
        public ActionResult Login(Login login)
        {
            if (ModelState.IsValid)
            {
                if (accountManager.ValidateUser(login.UserName, login.Password))
                {
                    var user = userManager.Fetch(login.UserName);

                //    FormsAuthentication.SetAuthCookie(login.UserName, login.RememberMe);

                    var identity = new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, login.UserName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Name, string.Format("{0} {1}", user.Forename, user.Surname)), 
                        }, 
                        DefaultAuthenticationTypes.ApplicationCookie,
                        ClaimTypes.NameIdentifier, 
                        ClaimTypes.Role);
                    // if you want roles, just add as many as you want here (for loop maybe?)

                    foreach (var role in user.Roles)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));
                    }
                    // tell OWIN the identity provider, optional
                    // identity.AddClaim(new Claim(IdentityProvider, "Simplest Auth"));

                    Authentication.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = true
                    }, identity);




                    using (var authService = AppHostBase.Instance.TryResolve<AuthService>())
                    {
                        authService.RequestContext = System.Web.HttpContext.Current.ToRequestContext();

                        var response = authService.Authenticate(new Auth
                        {
                            UserName = user.Username,
                            Password = login.Password,
                            RememberMe = login.RememberMe
                        });

                    }

                    if (user.Roles.Any(n => n.Name == "SCE"))
                    {
                        return RedirectToAction("Index", "Home", new { area = "" });
                    }

                    HttpContext.Session.Add("FileManager92ij098sduoisjd90", "/Uploads");
                    

                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                var errors = notificationService.FetchIssues();
                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }
            return View(login);
        }

        [HttpGet]
        public ActionResult UsernameReminder()
        {
            return View(new AccountReminder());
        }

        [HttpPost]
        public ActionResult UsernameReminder(AccountReminder accountReminder)
        {
            if (ModelState.IsValid)
            {

                if (accountManager.ResendUsername(accountReminder.EmailAddress))
                {
                    var successView = new SuccessView
                    {
                        Message = "Your username has been emailed to your registered email address.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = "Return to Login", URL = @Url.Action("Login", "Account")}

                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "" });
                }


                var errors = notificationService.FetchIssues();

                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }
            return View(new AccountReminder());
        }

        [HttpGet]
        public ActionResult ResendPassword()
        {
            return View(new AccountReminder());
        }

        [HttpPost]
        public ActionResult ResendPassword(AccountReminder accountReminder)
        {
            if (ModelState.IsValid)
            {
                if (accountManager.ResendPassword(accountReminder.EmailAddress))
                {
                    var successView = new SuccessView
                    {
                        Message = "A new password has been emailed to your registered email address.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = "Return to Login", URL = @Url.Action("Login", "Account")}

                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "" });
                }

                var errors = notificationService.FetchIssues();

                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }
            return View(new AccountReminder());
        }

        [HttpGet]
        [System.Web.Mvc.Authorize]
        public ActionResult Details()
        {
            return View(new ChangePassword());
        }

        [System.Web.Mvc.Authorize]
        [HttpGet]
        public ActionResult InterviewerDetails()
        {
            var user = userManager.Fetch(User.Identity.Name);
            var profile = userManager.FetchProfile(User.Identity.Name);
            var sceDetails = userManager.FetchSCEData(user.ID);


            var loggedInuser = userManager.Fetch(User.Identity.Name);

            sceDetails.Username = sceDetails.Username ?? loggedInuser.Username;
            sceDetails.Forename = sceDetails.Forename ?? loggedInuser.Forename;
            sceDetails.Surname = sceDetails.Surname ?? loggedInuser.Surname;
            sceDetails.Email = sceDetails.Email ?? loggedInuser.Email;
            sceDetails.Enabled = loggedInuser.Enabled;
            var allGroups = groupManager.FetchGroups();
            var dropDowns = new SCEFormData();

            ViewBag.GradesDropDown = dropDowns.Grades;
            ViewBag.TrustsDropDown = dropDowns.Trusts;

            ViewBag.GroupDropDown = allGroups;
            sceDetails.AllGroups = allGroups;
            sceDetails.BelongsToGroups = profile.Groups.Select(n => n.ID.ToString(CultureInfo.InvariantCulture)).ToArray();

            return View(sceDetails);
        }


        [System.Web.Mvc.Authorize]
        [HttpPost]
        public ActionResult InterviewerDetails(SCEData sceDetails)
        {
            if (ModelState.IsValid)
            {
                if (userManager.UpdateSCE(sceDetails))
                {
                    var successView = new SuccessView
                    {
                        Message = "Your details were successfully updated.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text ="Edit My Details", URL = @Url.Action("InterviewerDetails", "Account")},
                                new SuccessView.Link {Text = "Return to Home", URL = @Url.Action("Index", "Home")}
                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home");
                }

                var errors = notificationService.FetchIssues();
                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }

            var allGroups = groupManager.FetchGroups();
            var dropDowns = new SCEFormData();

            ViewBag.GradesDropDown = dropDowns.Grades;
            ViewBag.TrustsDropDown = dropDowns.Trusts;
            ViewBag.GroupDropDown = allGroups;
            sceDetails.AllGroups = allGroups;
            return View(sceDetails);
        }


        [HttpPost]
        [System.Web.Mvc.Authorize]//todo: look into using this https://github.com/colinangusmackay/Xander.PasswordValidator
        public ActionResult Details(ChangePassword changePassword)
        {
            if (ModelState.IsValid)
            {
                changePassword.Username = User.Identity.Name;

                if (accountManager.ChangePassword(changePassword))
                {
                    var successView = new SuccessView
                    {
                        Message = "Your password has been updated.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = "Return to Details page", URL = @Url.Action("Details", "Account")}
                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "" });
                }

                var errors = notificationService.FetchIssues();

                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }

            return View(changePassword);
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        [System.Web.Mvc.Authorize]
        public ActionResult Logout()
        {
           /* FormsAuthentication.SignOut();
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            Session.Abandon();*/
            Authentication.SignOut();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

          //  Session.();

            return RedirectToAction("Login");
        }

        [AjaxOnly]
        public CustomJsonResult FetchEmailSentLog(string username)
        {
            var emailsSentInformation = emailService.FetchEmailLog(username);
            var success = emailsSentInformation.HasContent();
            return new CustomJsonResult
            {
                Data = new { success = success, message = success ? string.Empty : string.Format("No emails have been sent for {0}.", username), results = emailsSentInformation }
            };

        }

        [AjaxOnly]
        public CustomJsonResult ResendAccountCreationEmail(string username)
        {
            var success = accountManager.ResendAccountCreationEmail(username);

            //TODO: pull errors here and pass in Json obj?

            return new CustomJsonResult
                {
                    Data = new { success = success, message = string.Empty, results = string.Empty }
                };
        }

    }
}

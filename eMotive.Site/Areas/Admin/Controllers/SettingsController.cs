using System.Web.Mvc;
using eMotive.Managers.Interfaces;
using eMotive.SCE.Common;
using eMotive.SCE.Common.ActionFilters;
using eMotive.Models.Objects.Search;
using eMotive.Models.Objects.StatusPages;
using eMotive.Search.Interfaces;
using eMotive.Services.Interfaces;
using eMotive.Services.Objects.Settings;
using ServiceStack;
using ServiceStack.Mvc;

namespace eMotive.SCE.Areas.Admin.Controllers
{
    public class SettingsController : ServiceStackController
    {
        private readonly INewsManager newsManager;
        private readonly IUserManager userManager;
        private readonly IRoleManager roleManager;
        private readonly ISearchManager searchManager;
        private readonly IEmailService emailService;
        private readonly IPageManager pageManager;
        private readonly IPartialPageManager partialPageManager;
        private readonly ISessionManager sessionManager;
        private readonly IeMotiveConfigurationService configurationService;

        public SettingsController(ISearchManager _searchManager, IUserManager _userManager, IRoleManager _rolemanager, 
                                  IPageManager _pageManager, IPartialPageManager _partialPageManager, INewsManager _newsManager, IEmailService _emailService, ISessionManager _sessionManager,
            IeMotiveConfigurationService _configurationService)
        {
            newsManager = _newsManager;
            userManager = _userManager;
            roleManager = _rolemanager;
            searchManager = _searchManager;
            emailService = _emailService;
            pageManager = _pageManager;
            partialPageManager = _partialPageManager;
            sessionManager = _sessionManager;
            configurationService = _configurationService;
        }

        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult Index()
        {
            return View();
        }

        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult Search()
        {
            var stats = new SearchStatistics {NumberOfDocuments = searchManager.NumberOfDocuments()};

            return View(stats);
        }

        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        [HttpGet]
        public ActionResult Site()
        {
            var settings = configurationService.FetchSettings();
            return View(settings);
        }

        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        [HttpPost]
        public ActionResult Site(Settings settings)
        {
            if (ModelState.IsValid)
            {
                if (configurationService.SaveSettings(settings))
                {
                    var successView = new SuccessView
                    {
                        Message = "Settings were saved.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = "Return to Settings Home", URL = @Url.Action("Index", "Settings")},
                                new SuccessView.Link {Text = "Return to Site Settings", URL = @Url.Action("Site", "Settings")}

                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "Admin" });
                }

                
            }

            return View(settings);
        }


        [AjaxOnly]
        [Common.ActionFilters.Authorize(Roles="Super Admin, Admin")]
        public CustomJsonResult ReindexAllDocuments()
        {
            searchManager.DeleteAll();
            newsManager.ReindexSearchRecords();
            roleManager.ReindexSearchRecords();
            userManager.ReindexSearchRecords();
            emailService.ReindexSearchRecords();
            pageManager.ReindexSearchRecords();
            sessionManager.ReindexSearchRecords();
            partialPageManager.ReindexSearchRecords();

            return new CustomJsonResult
            {
                Data = new { success = true }
            };
        }

    }
}

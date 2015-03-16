using System.Web.Mvc;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Pages;
using eMotive.Models.Objects.StatusPages;
using eMotive.Services.Interfaces;
using Extensions;
using ServiceStack.Mvc;

namespace eMotive.SCE.Areas.Admin.Controllers
{
    [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin, Moderator")]
    public class PagesController : ServiceStackController
    {
        private readonly IPartialPageManager pageManager;
        private readonly INotificationService notificationService;

        private readonly string[] searchType;

        public PagesController(IPartialPageManager _pageManager, INotificationService _notificationService)
        {
            pageManager = _pageManager;
            notificationService = _notificationService;
            searchType = new[] { "PartialPage" };
        }

        public ActionResult Index(PartialPageSearch pageSearch)
        {
            pageSearch.Type = searchType;

            var searchItem = pageManager.DoSearch(pageSearch);
            if (searchItem.Items.HasContent())
            {
                pageSearch.NumberOfResults = searchItem.NumberOfResults;
                pageSearch.Pages = pageManager.FetchRecordsFromSearch(searchItem);

                return View(pageSearch);
            }
            return View(new PartialPageSearch());
        }

        [HttpGet]
        public ActionResult Create()
        {

            return View(pageManager.New());
        }

        [HttpPost]
        public ActionResult Create(PartialPage page)
        {
            if (ModelState.IsValid)
            {
                int id;
                if (pageManager.Create(page, out id))
                {
                    var successView = new SuccessView
                    {
                        Message = "The new page was successfully created.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = "Edit new page", URL = @Url.Action("Edit", "Pages", new {key = page.Key})},
                                new SuccessView.Link {Text = "Return to pages home", URL = @Url.Action("Index", "Pages")}
                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "Admin" });
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
            return View(page);
        }

        [HttpGet]
        public ActionResult Edit(string key)
        {
            return View(pageManager.Fetch(key));
        }

        [HttpPost]
        public ActionResult Edit(PartialPage page)
        {
            if (ModelState.IsValid)
            {
                if (pageManager.Update(page))
                {
                    var successView = new SuccessView
                    {
                        Message =  "The partial page was successfully updated.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = string.Format("Return to Edit '{0}' partial page", page.Key), URL = Request.Url == null ? string.Empty : Request.Url.AbsoluteUri},
                                new SuccessView.Link {Text = "Return to pages Home", URL = @Url.Action("Index", "Pages")}

                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "Admin" });
                }

                var errors = notificationService.FetchIssues();
                foreach (var error in errors)
                {
                    ModelState.AddModelError("error", error);
                }
            }

            return View(page);
        }

    }
}

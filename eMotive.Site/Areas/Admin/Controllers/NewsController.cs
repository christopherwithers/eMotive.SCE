using System.Web.Mvc;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.News;
using eMotive.Models.Objects.StatusPages;
using eMotive.Models.Objects.Users;
using eMotive.Services.Interfaces;
using Extensions;
using ServiceStack.Mvc;

namespace eMotive.SCE.Areas.Admin.Controllers
{
    [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin, Moderator")]
    public class NewsController : ServiceStackController
    {
        private readonly INewsManager newsManager;
        private readonly INotificationService notificationService;

        private readonly string[] searchType;

        public NewsController(INewsManager _newsManager, INotificationService _notificationService)
        {
            newsManager = _newsManager;
            notificationService = _notificationService;
            searchType = new[] { "NewsItem" };
        }

        public ActionResult Index(NewsSearch newsSearch)
        {
            newsSearch.Type = searchType;

            var searchItem = newsManager.DoSearch(newsSearch);
            if (searchItem.Items.HasContent())
            {
                newsSearch.NumberOfResults = searchItem.NumberOfResults;
                newsSearch.NewsItems = newsManager.FetchRecordsFromSearch(searchItem);

                return View(newsSearch);
            }
            return View(new NewsSearch());
        }

        [HttpGet]
        public ActionResult Create()
        {

            return View(newsManager.New());
        }

        [HttpPost]
        public ActionResult Create(NewsItem newsItem)
        {
            if (ModelState.IsValid)
            {
                newsItem.Author = new User {Username = User.Identity.Name ?? string.Empty};
                int id;
                if (newsManager.Create(newsItem, out id))
                {
                    var successView = new SuccessView
                    {
                        Message = "The new News item was successfully created.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = "Edit new News Item", URL = @Url.Action("Edit", "News", new {id = id})},
                                new SuccessView.Link {Text = "Return to News Home", URL = "/Admin/News"}
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
            return View(newsItem);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            return View(newsManager.Fetch(id));
        }

        [HttpPost]
        public ActionResult Edit(NewsItem newsItem)
        {
            if (ModelState.IsValid)
            {
                newsItem.Author = new User {ID = newsItem.AuthorID};

                if (newsManager.Update(newsItem))
                {
                    var successView = new SuccessView
                    {
                        Message = "The news item was successfully updated.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = string.Format("Return to Edit '{0}' News Item", newsItem.Title), URL = Request.Url == null ? string.Empty : Request.Url.AbsoluteUri},
                                new SuccessView.Link {Text = "Return to News Home", URL = "/Admin/News"}

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

            return View(newsItem);
        }
    }
}

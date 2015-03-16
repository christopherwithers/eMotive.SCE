using System.Web.Mvc;
using eMotive.Models.Objects.StatusPages;
using ServiceStack.Mvc;

namespace eMotive.SCE.Areas.Admin.Controllers
{

    public class HomeController : ServiceStackController
    {
        //
        // GET: /Admin/Home/
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin, UGC")]
        public ActionResult Index()
        {


            return View();
        }

        public ActionResult Error()
        {
            ErrorView errorView;
            if (TempData["CriticalErrors"] != null)
            {
                errorView = TempData["CriticalErrors"] as ErrorView;
                TempData["CriticalErrors"] = TempData["CriticalErrors"];
            }
            else
            {
                errorView = new ErrorView
                {
                    ControllerName = "Home",
                    Errors = new[] { "An error occurred." }
                };
            }


            return View(errorView);
        }

        public ActionResult Success()
        {
            if (TempData["SuccessView"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var successView = TempData["SuccessView"] as SuccessView;
            TempData["SuccessView"] = TempData["SuccessView"];

            return View(successView);

        }

    }
}

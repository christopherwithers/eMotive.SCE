using System.Web.Mvc;
using ServiceStack.Mvc;

namespace eMotive.SCE.Areas.Admin.Controllers
{

    public class FormsController : ServiceStackController
    {
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin, UGC")]
        public ActionResult Index()
        {
            return View();
        }
    }
}

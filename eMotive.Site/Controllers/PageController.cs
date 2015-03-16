using System.Web.Mvc;
using ServiceStack.Mvc;

namespace eMotive.SCE.Controllers
{
    public class PageController : ServiceStackController
    {
        //
        // GET: /Page/

        public ActionResult Index()
        {
            return View();
        }

    }
}

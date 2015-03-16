using System.Web;
using System.Web.Mvc;

namespace eMotive.SCE.Common.ActionFilters
{
    public class AjaxOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
                throw new HttpException(404, "HTTP/1.1 404 Not Found");
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}
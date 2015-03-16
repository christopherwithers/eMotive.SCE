using System.Web.Mvc;

namespace eMotive.SCE.Common.ApplicationFilters
{
    public class ApplicationAttribute : ActionFilterAttribute
    {
        public string Name { get; set; }
    }
}
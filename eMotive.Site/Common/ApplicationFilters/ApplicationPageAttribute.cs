using System.Web.Mvc;

namespace eMotive.SCE.Common.ApplicationFilters
{
    public class ApplicationPageAttribute : ActionFilterAttribute
    {
        public string Name { get; set; }
    }
}
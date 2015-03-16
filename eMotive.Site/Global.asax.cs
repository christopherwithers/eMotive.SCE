using System.Web.Mvc;
using System.Web.Routing;
using eMotive.Search.Interfaces;
using ServiceStack.WebHost.Endpoints;

namespace eMotive.SCE
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

          //  DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
           // ModelValidatorProviders.Providers.Add(new FluentValidationModelValidatorProvider(new FunqValidatorFactory()));
           // FluentValidationModelValidatorProvider.Configure();

            new AppHost().Init();
        }
        /*
        public override void Dispose()
        {
            var searchManager = AppHostBase.Resolve<ISearchManager>();

            if (searchManager != null)
            {
                searchManager.Dispose();
            }

            base.Dispose();
        }*/
    }
}
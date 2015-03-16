using System.Web.Mvc;

namespace eMotive.SCE.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_UserSessions",
                "Admin/Signups/UserSessions/{username}/{id}",
                new { controller = "Signups", action = "UserSessions", username = UrlParameter.Optional, id = UrlParameter.Optional, }
                );
            context.MapRoute(
                "Admin_UserSlots",
                "Admin/Signups/UserSlots/{username}/{id}",
                new { controller = "Signups", action = "UserSlots", username = UrlParameter.Optional, id = UrlParameter.Optional, }
                );
            context.MapRoute(
                "Admin_Signup1",
                "Admin/Signups/{action}/{id}",
                new { controller = "Signups", action = "SignupDetails", id = UrlParameter.Optional, }
                );


        /*    context.MapRoute(
                "Admin_Signup2",
                "Admin/Signups/{action}/{id}",
                new { controller = "Signups", action = "GroupEdit", id = UrlParameter.Optional, }
                );*/




            context.MapRoute(
                "Admin_Users",
                "Admin/Users/{action}/{username}",
                new { controller = "Users", action = "Index", username = UrlParameter.Optional }
                );



            context.MapRoute(
                "Admin_Email",
                "Admin/Email/{action}/{key}",
                new { controller = "Email", action = "Index", key = UrlParameter.Optional }
                );

            context.MapRoute(
                "Admin_PartialPages",
                "Admin/Pages/{action}/{key}",
                new { controller = "Pages", action = "Index", key = UrlParameter.Optional }
                );
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller ="Home", action = "Index", id = UrlParameter.Optional }
                );




        }
    }
}

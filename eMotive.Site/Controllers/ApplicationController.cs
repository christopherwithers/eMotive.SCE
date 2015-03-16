using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ServiceStack.Mvc;

namespace eMotive.SCE.Controllers
{
    public class ApplicationController : ServiceStackController
    {
        /// <summary> 
        /// Renders out javascript 
        /// </summary> 
        /// <returns></returns> 
        [OutputCache(CacheProfile = "Cache1Hour")]
        [ActionName("js")]
        public ContentResult RenderJavascript()
        {
            return new ContentResult()
            {
                Content = GetLookupTables(),
                ContentType = "application/x-javascript"
            };
        }

        [NonAction]
        private string GetLookupTables()
        {
            var js = new StringBuilder();

            // list of keys that correspond to route URLS 
            var urls = new[] 
            {
                new { key = "FetchAllRoles", url = Url.RouteUrl("Admin_default", new { action = "GetAllRoles", controller = "Roles"}) },
                new { key = "CreateRole", url = Url.RouteUrl("Admin_default", new { action = "CreateRole", controller = "Roles"}) },
                new { key = "UpdateRole", url = Url.RouteUrl("Admin_default", new { action = "UpdateRole", controller = "Roles"}) },
                new { key = "DeleteRole", url = Url.RouteUrl("Admin_default", new { action = "DeleteRole", controller = "Roles"}) },

                new { key = "FetchUserNotes", url = Url.RouteUrl("Admin_default", new { action = "FetchUserNotes", controller = "Users"}) },
                new { key = "SaveUserNotes", url = Url.RouteUrl("Admin_default", new { action = "SaveUserNotes", controller = "Users"}) },
                new { key = "SessionAttendanceCertificate", url = Url.RouteUrl("Admin_default", new { action = "SessionAttendanceCertificate", controller = "Users"}) },
                new { key = "sendTrainingCertificate", url = Url.RouteUrl("Admin_default", new { action = "SessionAttendanceCertificate", controller = "Users"}) },
                new { key = "FetchSCETrainingInformation", url = Url.RouteUrl("Admin_default", new { action = "FetchSCETrainingInformation", controller = "Users"}) },
                /*
                new { key = "FetchUsers", url = Url.RouteUrl("Admin_default", new { action = "GetUsers", controller = "Users"}) },
                new { key = "CreateUser", url = Url.RouteUrl("Admin_default", new { action = "CreateUser", controller = "Users"}) },
                new { key = "UpdateUser", url = Url.RouteUrl("Admin_default", new { action = "UpdateUser", controller = "Users"}) },
                new { key = "DeleteUser", url = Url.RouteUrl("Admin_default", new { action = "DeleteUser", controller = "Users"}) },*/

               // new { key = "SignupToSlot", url = Url.RouteUrl("default", new { action = "SignupToSlot", controller = "Interviews" }) },
               // new { key = "CancelSignup", url = Url.RouteUrl("default", new { action = "CancelSignupToSlot", controller = "Interviews" }) },

                new { key = "SignupToSlotAdmin", url = Url.RouteUrl("Admin_default", new { action = "SignupToSlot", controller = "Signups" }) },
                new { key = "CancelSignupAdmin", url = Url.RouteUrl("Admin_default", new { action = "CancelSignupToSlot", controller = "Signups" }) },
                new { key = "FetchSignup", url = Url.RouteUrl("default", new { action = "FetchSignup", controller = "Signups" }) },
                new { key = "SaveSignup", url = Url.RouteUrl("default", new { action = "SaveSignup", controller = "Signups" }) },

                new { key = "FetchAllGroups", url = Url.RouteUrl("Admin_default", new { action = "FetchAllGroups", controller = "Signups" }) },
                new { key = "FetchSignupAng", url = Url.RouteUrl("Admin_default", new { action = "FetchSignup", controller = "Signups" }) },
                
                new { key = "FetchEmailSentLog", url = Url.RouteUrl("default", new { action = "FetchEmailSentLog", controller = "Account"}) },
                new { key = "ResendAccountCreationEmail", url = Url.RouteUrl("default", new { action = "ResendAccountCreationEmail", controller = "Account"}) },
                new { key = "DeleteUser", url = Url.RouteUrl("Admin_default", new { action = "DeleteUser", controller = "Users"}) },
                new { key = "FetchApplicantSignups", url = Url.RouteUrl("Admin_default", new { action = "FetchApplicantSignups", controller = "Users"}) },
                new { key = "FetchApplicantData", url = Url.RouteUrl("Admin_default", new { action = "FetchApplicantData", controller = "Users"}) },
                
                new { key = "ReindexAllDocuments", url = Url.RouteUrl("Admin_default", new { action = "ReindexAllDocuments", controller = "Settings"}) }
            };

            js.AppendLine("var Routes = (function(){");

            js.AppendLine("// URL Lookuptable");
            js.AppendLine("var urls=function(url) {");
            js.AppendLine("var lookupTable = " + new JavaScriptSerializer().Serialize(urls.ToDictionary(x => x.key, x => x.url)) + ";");
            js.AppendLine("return lookupTable[url];");
            js.AppendLine("};");

            js.AppendLine("return {");
            js.AppendLine("URL: urls");
            js.AppendLine("};");

            js.AppendLine("})();");

            return js.ToString();

        }




    }
}

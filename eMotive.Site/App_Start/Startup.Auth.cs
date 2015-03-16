using System;
using System.Security.Claims;
using System.Web.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace eMotive
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                CookieName = "UoBSCE",
                ExpireTimeSpan = TimeSpan.FromMinutes(20),
                SlidingExpiration = true
               // CookieSecure = CookieSecureOption.Always
            });

         //   app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            
        //    app.
            //app.UseGoogleAuthentication();
           // app.MapSignalR();
        }
    }
}
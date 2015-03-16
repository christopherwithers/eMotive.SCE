using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(eMotive.Startup))]
namespace eMotive
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {            // Enable the application to use a cookie to store information for the signed in user
            ConfigureAuth(app);

            app.MapSignalR();
        }
    }
}
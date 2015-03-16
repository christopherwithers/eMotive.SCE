using System;
using System.Web;
using eMotive.Services.Interfaces;
using ServiceStack.WebHost.Endpoints;

namespace eMotive.SCE.Core.Modules
{
    public class ErrorLogModule : IHttpModule
    {
        private INotificationService _logService;

        public void Init(HttpApplication context)
        {
            context.EndRequest += LogRequest;
        }

        private void LogRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;

            if (app == null || app.Context.Handler == null) return;

            if (app.Context.Handler is System.Web.Mvc.MvcHandler)
            {
                _logService = AppHostBase.Instance.TryResolve<INotificationService>();

                if (_logService != null) _logService.CommitDatabaseLog();
            }
        }

        public void Dispose()
        {
            //do we need to do anything here?
        }
    }
}
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using eMotive.SCE.Controllers;
using eMotive.IoCBindings.Funq;
using eMotive.Models.Validation.Account;
using eMotive.Services.Interfaces;
using eMotive.Services.Objects;
using FluentValidation.Mvc;
using Funq;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.Configuration;
using ServiceStack.Mvc;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;


namespace eMotive.SCE
{
    public class AppHost : AppHostBase
    {
        public AppHost() : base("SCE Web Services", typeof(Api.SessionService).Assembly) { }

        public override void Configure(Container container)
        {
            FunqBindings.Configure(container);

            container.Register<ICacheClient>(new MemoryCacheClient
            {
                FlushOnDispose = false
            });
            container.Register<ISessionFactory>(c => new SessionFactory(c.Resolve<ICacheClient>()));

            var dictDir = @"\Core\SpellChecker\";
            container.Register<IDictionaryService>(c => new DictionaryService(HttpContext.Current.Server.MapPath(string.Format("~{0}", dictDir)))).ReusedWithin(ReuseScope.Container);
            

          //  SetConfig(new EndpointHostConfig { DebugMode = true });
            JsConfig.DateHandler = JsonDateHandler.ISO8601;

            FluentValidationModelValidatorProvider.Configure();

            Plugins.Add(new AuthFeature(
                            () => new AuthUserSession(),
                            new IAuthProvider[] { new CustomCredentialsAuthProvider() }
                        ));


            ControllerBuilder.Current.SetControllerFactory(new FunqControllerFactory(container));
            ServiceStackController.CatchAllController = reqCtx => container.TryResolve<AccountController>();
        }

        private class CustomCredentialsAuthProvider : CredentialsAuthProvider
        {
            public override object Authenticate(IServiceBase authService, IAuthSession session, Auth request)
            {
                var userName = request.UserName;
                var password = request.Password;

                if (!LoginMatchesSession(session, userName))
                {
                    authService.RemoveSession();
                    session = authService.GetSession();
                }

                if (TryAuthenticate(authService, userName, password))
                {
                    authService.SaveSession(session, SessionExpiry);
                    if (session.UserAuthName == null)
                        session.UserAuthName = userName;

                    OnAuthenticated(authService, session, null, null);

                    return new AuthResponse
                    {
                        UserName = userName,
                        SessionId = session.Id,
                        ReferrerUrl = RedirectUrl
                    };
                }

                throw new HttpError(HttpStatusCode.BadRequest, "400", "wrong credentials");

            }

            public override bool TryAuthenticate(IServiceBase authService, string userName, string password)
            {//TODO: As we're calling this when authed, we don't really need to re-auth?
              //  if (!Membership.ValidateUser(userName, password)) return false;

                var session = (AuthUserSession)authService.GetSession(false);//(AuthUserSession)

                session.UserAuthId = userName;
                session.IsAuthenticated = true;

                // add roles 
             /*   session.Roles = new List<string>();
                if (session.UserAuthId == "admin") session.Roles.Add(RoleNames.Admin);
                session.Roles.Add("User");*/

                return true;
            }
        }
    }
}
using System.Configuration;
using AutoMapper;
using Cache.Interfaces;
using Cache.Objects;
using eMotive.Managers.Interfaces;
using eMotive.Managers.Objects;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;
using eMotive.Services;
using eMotive.Services.Interfaces;
using eMotive.Services.Objects;
using Funq;

namespace eMotive.IoCBindings.Funq
{
    public static class FunqBindings
    {
        public static void Configure(Container container)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["live"].ConnectionString ?? string.Empty;
            var enableLogging = ConfigurationManager.AppSettings["Logging"] ?? "False";
            var luceneIndex = ConfigurationManager.AppSettings["LuceneIndex"] ?? string.Empty;
            container.Register(c => Mapper.Engine);

            container.Register<ICache>(c => new DevNullCache());

            container.Register<IUserRepository>(c => new MySqlUserRepository(connectionString)).ReusedWithin(ReuseScope.Request);
            container.Register<IRoleRepository>(c => new MySqlRoleRepository(connectionString)).ReusedWithin(ReuseScope.Request);
            container.Register<INewsRepository>(c => new MySqlNewsRepository(connectionString)).ReusedWithin(ReuseScope.Request);
            container.Register<IPageRepository>(c => new MySqlPageRepository(connectionString)).ReusedWithin(ReuseScope.Request);
            container.Register<ISessionRepository>(c => new MySqlSessionRepository(connectionString)).ReusedWithin(ReuseScope.Request);
            container.Register<IFormRepository>(c => new MySqlFormRepository(connectionString)).ReusedWithin(ReuseScope.Request);

            container.Register<IUserManager>(c => new UserManager(c.Resolve<IUserRepository>(), c.Resolve<IAccountManager>(), c.Resolve<ISearchManager>(), c.Resolve<IGroupManager>())
            {
                notificationService = c.Resolve<INotificationService>(),
                roleManager = c.Resolve<IRoleManager>()
            }).ReusedWithin(ReuseScope.Request);


            container.Register<IFormManager>(c => new FormManager(c.Resolve<IFormRepository>())
            {
                NotificationService = c.Resolve<INotificationService>(),
                Mapper = c.Resolve<IMappingEngine>()
            }).ReusedWithin(ReuseScope.Request);

            container.Register<IRoleManager>(c => new RoleManager(c.Resolve<IRoleRepository>(), c.Resolve<ISearchManager>())
            {
                notificationService = c.Resolve<INotificationService>(),
            }).ReusedWithin(ReuseScope.Request);

            container.Register<INewsManager>(c => new NewsManager(c.Resolve<INewsRepository>(), c.Resolve<ISearchManager>(), c.Resolve<IUserManager>())
            {
                notificationService = c.Resolve<INotificationService>(),
            }).ReusedWithin(ReuseScope.Request);


            container.Register<IPageManager>(c => new PageManager(c.Resolve<IPageRepository>(), c.Resolve<ISearchManager>())
            {
                notificationService = c.Resolve<INotificationService>(),
            }).ReusedWithin(ReuseScope.Request);


            container.Register<IPartialPageManager>(c => new PartialPageManager(c.Resolve<IPageRepository>(), c.Resolve<ISearchManager>())
            {
                notificationService = c.Resolve<INotificationService>(),
            }).ReusedWithin(ReuseScope.Request);

            container.Register<ISessionManager>(c => new SessionManager(c.Resolve<ISessionRepository>(), c.Resolve<IUserManager>(), c.Resolve<ISearchManager>(), c.Resolve<IFormManager>())
            {
                configurationService = c.Resolve<IeMotiveConfigurationService>(),
                notificationService = c.Resolve<INotificationService>(),
                emailService = c.Resolve<IEmailService>(),
                cache = c.Resolve<ICache>()
            }).ReusedWithin(ReuseScope.Request);

            container.Register<IGroupManager>(c => new GroupManager(c.Resolve<ISessionRepository>())
            {
                notificationService = c.Resolve<INotificationService>(),
            }).ReusedWithin(ReuseScope.Request);



            //container.Register<IeMotiveConfigurationService>(c => new eMotiveConfigurationServiceWebConfig()).ReusedWithin(ReuseScope.Request);
            container.Register<IeMotiveConfigurationService>(c => new eMotiveConfigurationServiceMySQL(connectionString/*, c.Resolve<ICacheClient>()*/)).ReusedWithin(ReuseScope.Container);
            container.Register<INotificationService>(c => new NotificationService(connectionString, enableLogging)).ReusedWithin(ReuseScope.Request);
            container.Register<IEmailService>(c => new EmailService(c.Resolve<IeMotiveConfigurationService>(), c.Resolve<INotificationService>(), c.Resolve<IDocumentManagerService>(), c.Resolve<ISearchManager>(), connectionString)).ReusedWithin(ReuseScope.Request);
            container.Register<IReportService>(c => new ReportService(connectionString)).ReusedWithin(ReuseScope.Request);
            container.Register<IDocumentManagerService>(c => new DocumentManagerService(connectionString)
            {
                notificationService = c.Resolve<INotificationService>(),
            }).ReusedWithin(ReuseScope.Request);
            container.Register<IAccountManager>(c => new AccountManager(connectionString, c.Resolve<IUserRepository>())
            {
                emailService = c.Resolve<IEmailService>(),
                notificationService = c.Resolve<INotificationService>(),
                configurationService = c.Resolve<IeMotiveConfigurationService>()
            }).ReusedWithin(ReuseScope.Request);

            container.Register<ISearchManager>(c => new SearchManager(luceneIndex)).ReusedWithin(ReuseScope.Container);
        }
    }
}

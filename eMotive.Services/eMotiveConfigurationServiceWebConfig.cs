using System.Configuration;
using System.Security.Claims;
using System.Web;
using eMotive.Services.Interfaces;
using eMotive.Services.Objects.Settings;

namespace eMotive.Services
{
    public class eMotiveConfigurationServiceWebConfig : IeMotiveConfigurationService
    {
        public string EmailFromAddress()
        {
            return ConfigurationManager.AppSettings["MailFromAddress"] ?? string.Empty;
        }

        public bool EmailsEnabled()
        {
            var emailEnabledString = ConfigurationManager.AppSettings["DisableEmails"] ?? "True";
            bool emailEnabled;
            if (!bool.TryParse(emailEnabledString, out emailEnabled))
                emailEnabled = true;

            return emailEnabled;
        }

        public int MaxLoginAttempts()
        {
            const int defaultValue = 5;
            var attemptString = ConfigurationManager.AppSettings["MaxLoginAttempts"] ?? string.Empty;

            if (string.IsNullOrEmpty(attemptString))
                return defaultValue;

            int attempts;

            return int.TryParse(attemptString, out attempts) ? attempts : defaultValue;
        }

        public int LockoutTimeInMinutes()
        {
            const int defaultValue = 15;
            var lockoutTimeString = ConfigurationManager.AppSettings["LockoutTimeMinutes"] ?? string.Empty;

            if (string.IsNullOrEmpty(lockoutTimeString))
                return 15;

            int lockoutTime;

            return int.TryParse(lockoutTimeString, out lockoutTime) ? lockoutTime : defaultValue;
        }

        public string SiteName()
        {
            return ConfigurationManager.AppSettings["SiteName"] ?? string.Empty;
        }

        public string SiteURL()
        {
            return ConfigurationManager.AppSettings["SiteURL"] ?? string.Empty;
        }

        public string GoogleAnalytics()
        {
           // throw new System.NotImplementedException();
            return string.Empty;
        }

        public string MetaTags()
        {
           // throw new System.NotImplementedException();
            return string.Empty;
        }

        public bool AllowWithdrawals()
        {
            throw new System.NotImplementedException();
        }

        public bool SaveSettings(Settings settings)
        {
            throw new System.NotImplementedException();
            //add message to notification service here?
        }

        public Settings FetchSettings()
        {
            throw new System.NotImplementedException();
        }

        public string GetClientIpAddress()
        {
            var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }

        public string GetLoggedInUsername()
        {
            return !HttpContext.Current.User.Identity.IsAuthenticated ? string.Empty : ((ClaimsIdentity)HttpContext.Current.User.Identity).Name;
        }
    }
}

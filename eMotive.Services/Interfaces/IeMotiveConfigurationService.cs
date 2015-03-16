using eMotive.Services.Objects.Settings;

namespace eMotive.Services.Interfaces
{
    public interface IeMotiveConfigurationService
    {
      /*  string PusherID();
        string PusherKey();
        string PusherSecret();*/

        string EmailFromAddress();
        bool EmailsEnabled();

        int MaxLoginAttempts();
        int LockoutTimeInMinutes();

        string SiteName();
        string SiteURL();

        string GoogleAnalytics();
        string MetaTags();

        bool AllowWithdrawals();

        bool SaveSettings(Settings settings);
        Settings FetchSettings();

        string GetClientIpAddress();

        string GetLoggedInUsername();
    }
}

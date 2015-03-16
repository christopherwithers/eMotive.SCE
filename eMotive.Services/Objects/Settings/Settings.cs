using eMotive.Services.Objects.Validation;
using ServiceStack.FluentValidation.Attributes;

namespace eMotive.Services.Objects.Settings
{
    [Validator(typeof(SettingsValidator))]
    public class Settings
    {
        public string SiteName { get; set; }
        public string SiteURL { get; set; }
        public bool DisableEmails { get; set; }
        public int MaxLoginAttempts { get; set; }
        public int LockoutTimeMinutes { get; set; }
        public string MailFromAddress { get; set; }
        public string GoogleAnalytics { get; set; }
        public string MetaTags { get; set; }
        public bool AllowWithdrawals { get; set; }
    }

}

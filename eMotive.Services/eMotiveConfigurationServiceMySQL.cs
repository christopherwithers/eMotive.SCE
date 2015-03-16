using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Transactions;
using System.Web;
using Dapper;
using eMotive.Services.Interfaces;
using eMotive.Services.Objects.Settings;
using MySql.Data.MySqlClient;


namespace eMotive.Services
{
    public class eMotiveConfigurationServiceMySQL : IeMotiveConfigurationService
    {
        private readonly string _connectionString;
        private IDbConnection _connection;
        private Settings _settings;

        private object objLock = new object();


        public eMotiveConfigurationServiceMySQL(string connectionString)
        {
            _connectionString = connectionString;

            if (!GetSettings())
                throw new Exception("Could not fetch site settings.");
        }


        private bool GetSettings()
        {
            using (var cn = Connection)
            {
                //check exists, else create!
                var results = cn.Query<Settings>("SELECT * FROM `settings` LIMIT 1;").SingleOrDefault();

                if (results == null)
                    return false;

                _settings = results;

                return true;
            }
        }

        internal IDbConnection Connection
        {
            get
            {
                return _connection ?? new MySqlConnection(_connectionString);
            }
        }

        internal Settings Settings()
        {
            if (_settings != null)
                return _settings;

            if (GetSettings())
                return _settings;

            throw new Exception("Could not fetch site settings.");
        }

        public string EmailFromAddress()
        {
            return Settings().MailFromAddress;
        }

        public bool EmailsEnabled()
        {
            return !Settings().DisableEmails;
        }

        public int MaxLoginAttempts()
        {
            return Settings().MaxLoginAttempts;
        }

        public int LockoutTimeInMinutes()
        {
            return Settings().LockoutTimeMinutes;
        }

        public string SiteName()
        {
            return Settings().SiteName;
        }

        public string SiteURL()
        {
            return Settings().SiteURL;
        }

        public string GoogleAnalytics()
        {
            return Settings().GoogleAnalytics;
        }

        public string MetaTags()
        {
            return Settings().MetaTags;
        }

        public bool AllowWithdrawals()
        {
            return Settings().AllowWithdrawals;
        }

        /*        private bool GetSettings()
        {
            using (var cn = Connection)
            {
                using (var transactionScope = new TransactionScope())
                {
                    string sql = "SELECT CAST(Count(FOUND_ROWS()) AS UNSIGNED INTEGER) FROM `settings`;";

                    if (cn.Query<ulong>(sql).SingleOrDefault() > 0)
                    {
                        //check exists, else create!
                        var results = cn.Query<Settings>("SELECT * FROM `settings`;").SingleOrDefault();

                        if (results == null)
                            return false;

                    }
                    else
                    {
                        {
                        }

                        _settings = results;

                        transactionScope.Complete();

                        return true;
                    }
                }
            }
        }*/
        public bool SaveSettings(Settings settings)
        {
            lock (objLock)
            {
                using (var cn = Connection)
                {
                    var success = false;

                    using (var transactionScope = new TransactionScope())
                    {
                        string sql = "SELECT CAST(Count(FOUND_ROWS()) AS UNSIGNED INTEGER) FROM `settings`;";

                        if (cn.Query<ulong>(sql).SingleOrDefault() > 0)
                        {//todo: check more than 1, if so, delete and reinsert?
                            success = cn.Execute("UPDATE `settings` SET `SiteName`=@SiteName,`SiteURL`=@SiteURL,`DisableEmails`=@DisableEmails,`MaxLoginAttempts`=@MaxLoginAttempts,`LockoutTimeMinutes`=@LockoutTimeMinutes,`MailFromAddress`=@MailFromAddress,`GoogleAnalytics`=@GoogleAnalytics,`MetaTags`=@MetaTags, `AllowWithdrawals`=@AllowWithdrawals LIMIT 1;", settings) > 0;

                        }
                        else
                        {
                            success = cn.Execute("INSERT INTO `settings` (`SiteName`,`SiteURL`,`DisableEmails`,`MaxLoginAttempts`,`LockoutTimeMinutes`,`MailFromAddress`,`GoogleAnalytics`,`MetaTags`, `AllowWithdrawals`) VALUES (@SiteName, @SiteURL, @DisableEmails, @MaxLoginAttempts, @LockoutTimeMinutes, @MailFromAddress, @GoogleAnalytics, @MetaTags, @AllowWithdrawals);", settings) > 0;
                        }

                        if (success)
                        {
                            _settings = settings;
                        }

                        transactionScope.Complete();
                        return success;
                    }
                }
            }
        }

        public Settings FetchSettings()
        {
            return Settings();
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

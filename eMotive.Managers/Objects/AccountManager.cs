using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using Extensions;
using MySql.Data.MySqlClient;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Account;
using eMotive.Models.Objects.Users;
using eMotive.Repository;
using eMotive.Repository.Interfaces;
using eMotive.Services.Interfaces;


namespace eMotive.Managers.Objects
{
    public class AccountManager : IAccountManager
    {
        private readonly IUserRepository userRepository;

        private readonly string connectionString;

        public AccountManager(string _connectionString, IUserRepository _userRepository)
        {
            connectionString = _connectionString;
            userRepository = _userRepository;

            AutoMapperManagerConfiguration.Configure();
        }


        public IEmailService emailService { get; set; }

        public INotificationService notificationService { get; set; }

        public IeMotiveConfigurationService configurationService { get; set; }

        public bool ValidateUser(string _username, string _password)
        {
            var maxAttempts = configurationService.MaxLoginAttempts();
            if (LoginAttemptCount(_username) >= maxAttempts)
            {
                notificationService.AddIssue("Your account has been locked for 15 minutes.");
                return false;
            }

            var user = userRepository.Fetch(_username, UserField.Username);
            if (user == null)
            {
                notificationService.AddIssue("Your username or password was not recognised.");
                LoginAttempt(_username);
                notificationService.AddIssue(string.Format("Login attempt {0} of {1}.", LoginAttemptCount(_username), maxAttempts));

                return false;
            }

            var salt = userRepository.GetSalt(_username);

            if (string.IsNullOrEmpty(salt))
            {
                notificationService.Log(string.Format("Account Manager: An error occured while fetching the salt for user '{0}'", _username));
                notificationService.AddError("An error occurred when trying to log you in. An administrator has been notified of this issue.");
                return false;
            }

            var password = EncryptPassword(salt, _password);

            var success = userRepository.ValidateUser(_username, password);

            if (!user.Enabled)
            {
                notificationService.AddIssue("Your account has been disabled.");
                return false;
            }

            if (userRepository.UserHasWithdrawn(user.ID))
            {
                notificationService.AddIssue("You have withdrawn from the MMI application process.");
                return false;
            }

            if (success)
                return true;

            notificationService.AddIssue("The entered username and password were not recognised.");
            LoginAttempt(_username);
            notificationService.AddIssue(string.Format("Login attempt {0} of {1}.", LoginAttemptCount(_username), maxAttempts));
            return false;
        }

        public bool WithdrawUser(int userId)
        {
            return userRepository.WithdrawUser(userId);
        }

        public bool UserHasWithdrawn(int userId)
        {
            return userRepository.UserHasWithdrawn(userId);
        }

        public bool CreateNewAccountPassword(User _user)
        {
            string salt;
            string plainPassword;
            var password = GeneratePassword(out salt, out plainPassword);
            var success = userRepository.SavePassword(_user.ID, salt, password);

            if (success)
            {
                var replacements = new Dictionary<string, string>(4)
                    {
                        {"#forename#", _user.Forename},
                        {"#surname#", _user.Surname},
                        {"#username#", _user.Username},
                        {"#password#", plainPassword},
                        {"#sitename#", configurationService.SiteName()},
                        {"#siteurl#", configurationService.SiteURL()},
                        {"#accounttype#", _user.Roles.HasContent() ? _user.Roles.First().Name : string.Empty},
                    };

         //       var profile = userRepository.FetchProfile(_user.Username);


                var key = string.Empty;

                if (_user.Roles.Any(n => n.Name == "Admin"))
                    key = "CreateAdminAccount";

                if (_user.Roles.Any(n => n.Name == "UGC"))
                    key = "CreateUGCAccount";

                if (_user.Roles.Any(n => n.Name == "SCE"))
                {
                    key = "CreateSCEAccount";
                    var scedata = userRepository.FetchSceData(_user.ID);
                    if(scedata != null)
                        replacements.Add("#title#", scedata.Title);
                }
                if (emailService.SendMail(key, _user.Email, replacements))
                {
                    emailService.SendEmailLog(key, _user.Username);
                    return true;
                }

                return true;
            }
            notificationService.Log(string.Format("Account Manager: An error occured while trying to email the account details to  '{0}'", _user.Username));
            notificationService.AddError(string.Format("An error occured while trying to email the account details to '{0}'.", _user.Username));
            return false;
        }

        public bool ResendAccountCreationEmail(string _username)
        {
            var user = userRepository.Fetch(_username, UserField.Username);

            if (user == null)
                return false;
            
            var profile = userRepository.FetchProfile(_username);

            string salt;
            string plainPassword;
            var password = GeneratePassword(out salt, out plainPassword);
            var success = userRepository.SavePassword(user.ID, salt, password);

            if (success)
            {
                var replacements = new Dictionary<string, string>(4)
                    {
                        {"#forename#", user.Forename},
                        {"#surname#", user.Surname},
                        {"#username#", user.Username},
                        {"#password#", plainPassword},
                        {"#sitename#", configurationService.SiteName()},
                        {"#siteurl#", configurationService.SiteURL()}
                    };


                var key = string.Empty;

                if (user.Roles.Any(n => n.Name == "Admin"))
                    key = "CreateAdminAccount";

                if (user.Roles.Any(n => n.Name == "UGC"))
                    key = "CreateUGCAccount";

                if (user.Roles.Any(n => n.Name == "SCE"))
                {
                    key = "CreateSCEAccount";
                    var scedata = userRepository.FetchSceData(user.ID);
                    if (scedata != null)
                        replacements.Add("#title#", scedata.Title);
                }



                if (emailService.SendMail(key, user.Email, replacements))
                {
                    emailService.SendEmailLog(key, user.Username);
                    return true;
                }
            }
            notificationService.Log(string.Format("Account Manager: An error occured while trying to email the account details to  '{0}'", user.Username));
            notificationService.AddError(string.Format("An error occured while trying to email the account details to '{0}'.", user.Username));
            return false;
        }

        /// <summary>
        /// Generates a new password and emails it to a user
        /// </summary>
        /// <param name="_email">An email address</param>
        /// <returns>A bool indicating if the operation was a success</returns>
        public bool ResendPassword(string _email)
        {
            var user = userRepository.Fetch(_email, UserField.Email);
            if (user == null)
            {
                notificationService.AddIssue("Your email address was not recognised.");
                return false;
            }

            string salt;
            string plainPassword;
            var password = GeneratePassword(out salt, out plainPassword);
            var success = userRepository.SavePassword(user.ID, salt, password);

            if (success)
            {
                var replacements = new Dictionary<string, string>(4)
                    {
                        {"#forename#", user.Forename},
                        {"#surname#", user.Surname},
                        {"#username#", user.Username},
                        {"#ip#", configurationService.GetClientIpAddress()},
                        {"#password#", plainPassword},
                        {"#date#", DateTime.Now.ToString("dddd d MMMM yyyy")},
                        {"#sitename#", configurationService.SiteName()},
                        {"#siteurl#", configurationService.SiteURL()}
                    };

                const string key = "UserResetPassword";

                if (user.Roles.Any(n => n.Name == "SCE"))
                {
                    var scedata = userRepository.FetchSceData(user.ID);
                    if (scedata != null)
                        replacements.Add("#title#", scedata.Title);
                }

                if (emailService.SendMail(key, _email, replacements))
                {
                    emailService.SendEmailLog(key, user.Username);
                    return true;
                }
            }

            notificationService.Log(string.Format("Account Manager: An error occured while trying to resend a password to  '{0}'", user.Username));
            notificationService.AddError("An error occured while trying to generate a new password.");
            return false;
        }

        public bool ResendUsername(string _email)
        {
            var user = userRepository.Fetch(_email, UserField.Email);
            if (user == null)
            {
                notificationService.AddIssue("Your email address was not recognised.");
                return false;
            }
            var replacements = new Dictionary<string, string>(4)
                {
                    {"#forename#", user.Forename},
                    {"#surname#", user.Surname},
                    {"#username#", user.Username},
                    {"#ip#", configurationService.GetClientIpAddress()},
                    {"#date#", DateTime.Now.ToString("dddd d MMMM yyyy")},
                    {"#sitename#", configurationService.SiteName()},
                    {"#siteurl#", configurationService.SiteURL()}
                };

            if (user.Roles.Any(n => n.Name == "SCE"))
            {
                var scedata = userRepository.FetchSceData(user.ID);
                if (scedata != null)
                    replacements.Add("#title#", scedata.Title);
            }

            const string key = "UserRequestUsername";

            if (emailService.SendMail(key, _email, replacements))
            {
                emailService.SendEmailLog(key, user.Username);
                return true;
            }

            notificationService.Log(string.Format("Account Manager: An error occured while trying to resend username to  '{0}'", _email));
            notificationService.AddError("An error occured while trying to email a username reminder.");
            return false;
        }


        public bool ChangePassword(ChangePassword _changePassword)
        {
            var user = userRepository.Fetch(_changePassword.Username, UserField.Username);
            if (user == null)
            {
                notificationService.Log(string.Format("Account Manager: An error occurred while trying to change a users password. The username '{0}' did not belong to a user account.", _changePassword.Username));
                notificationService.AddIssue("An error occurred while trying to change your password. Your password has not been changed.");
                return false;
            }

            var salt = userRepository.GetSalt(_changePassword.Username);

            if (string.IsNullOrEmpty(salt))
            {
                notificationService.Log(string.Format("Account Manager: An error occured while fetching the salt for user '{0}'", _changePassword.Username));
                notificationService.AddError("An error occurred when trying to log you in. An administrator has been notified of this issue.");
                return false;
            }

            var password = EncryptPassword(salt, _changePassword.CurrentPassword);

            if (!userRepository.ValidateUser(_changePassword.Username, password))
            {
                notificationService.AddIssue("Your current password was entered incorrectly.");

                return false;
            }

            var encryptedPassword = ChangePassword(out salt, _changePassword.NewPassword);

            if (userRepository.SavePassword(user.ID, salt, encryptedPassword))
                return true;

            notificationService.AddIssue("An error occurred while trying to change your password. Your password has not been changed.");

            return false;
        }


        private string GeneratePassword(out string _salt, out string _plainPassword)
        {
            var plainPassword = _plainPassword = GenerateAuthCode().Substring(0, 8);

            _salt = GenerateAuthCode();
            return EncryptPassword(_salt, plainPassword);
        }

        private string ChangePassword(out string _salt, string _newPassword)
        {
            _salt = GenerateAuthCode();
            return EncryptPassword(_salt, _newPassword);
        }

        private string EncryptPassword(string _salt, string _password)
        {
            var tempPassword = string.Concat(_password, _salt);

            MD5 md5 = new MD5CryptoServiceProvider();
            var originalBytes = Encoding.Default.GetBytes(tempPassword);
            var encodedBytes = md5.ComputeHash(originalBytes);

            return Regex.Replace(BitConverter.ToString(encodedBytes), "-", "");
        }

        private string GenerateAuthCode()
        {
            var guidResult = Guid.NewGuid().ToString();

            guidResult = guidResult.Replace("-", string.Empty);

            return guidResult.Substring(0, 12);
        }

        //todo: have a accountRepository and have all db code there?
        private void LoginAttempt(string _username)
        {
            var ipAddress = configurationService.GetClientIpAddress();

            if (string.IsNullOrEmpty(ipAddress))
            {
                notificationService.Log(string.Format("AccountManager: Could not resolve ip address for user '{0}' on failed login attempt.", _username));
                return;
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var sql = "INSERT INTO `loginattempts` (`IP`,`occurred`) VALUES (@ip, @occurred);";
                connection.Execute(sql, new { ip = ipAddress, occurred = DateTime.Now });
            }
        }

        private int LoginAttemptCount(string _username)
        {
            var ipAddress = configurationService.GetClientIpAddress();
            var lockoutTime = configurationService.LockoutTimeInMinutes();

            if (string.IsNullOrEmpty(ipAddress))
            {
                notificationService.Log(string.Format("AccountManager: Could not resolve ip address for user '{0}' on failed login attempt.", _username));
                return -1;
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "SELECT CAST(COUNT(*)AS UNSIGNED INTEGER) FROM `loginattempts` WHERE `IP`=@ip AND `Occurred` > @occurred;";
                var id = connection.Query<ulong>(sql,
                    new
                    {
                        ip = ipAddress,
                        occurred = DateTime.Now.AddMinutes(-lockoutTime)
                    }).SingleOrDefault();

                return Convert.ToInt32(id);
            }


        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Transactions;
using Dapper;
using Extensions;
using MySql.Data.MySqlClient;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects.Signups;
using eMotive.Repository.Objects.Users;

namespace eMotive.Repository.Objects
{//https://github.com/ServiceStack/ServiceStack.OrmLite
    //http://stackoverflow.com/questions/14480237/servicestack-ormlite-repository-pattern
    //https://groups.google.com/forum/#!msg/servicestack/1pA41E33QII/R-trWwzYgjEJ
    //http://xunitpatterns.com/Obscure%20Test.html#General
    //http://xunitpatterns.com/Obscure%20Test.html#General
    public class MySqlUserRepository : IUserRepository
    {
        private readonly string connectionString;
        private readonly string userFields;

        public MySqlUserRepository(string _connectionString)
        {
            connectionString = _connectionString;
            userFields = "`id`, `username`, `forename`, `surname`, `email`, `enabled`, `archived`";
        }

        public User New()
        {
            return new User();
        }

        public User Fetch(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = string.Format("SELECT {0} FROM `users` WHERE `id`=@id;", userFields);

                var user = connection.Query<User>(sql, new {id = _id}).SingleOrDefault();

                if (user != null)
                {
                    sql =
                        "SELECT b.* FROM `userHasRoles` a INNER JOIN `roles` b ON a.`roleID`=b.`id` WHERE a.`userId`=@userId";

                    user.Roles = connection.Query<Role>(sql, new {userId = _id});
                }

                return user;
            }
        }

        public User Fetch(string _value, UserField _field)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = string.Format("SELECT {0} FROM `users` WHERE `{1}`=@val;", userFields, _field);

                var user = connection.Query<User>(sql, new { val = _value }).SingleOrDefault();

                if (user != null)
                {
                    sql =
                        "SELECT b.* FROM `userHasRoles` a INNER JOIN `roles` b ON a.`roleID`=b.`id` WHERE a.`userId`=@userId;";

                    user.Roles = connection.Query<Role>(sql, new { userId = user.ID });
                }

                return user;
            }
        }

        public string FetUserNotes(string _username)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "SELECT `notes` FROM `users` WHERE `username`=@username;";

                var user = connection.Query<string>(sql, new { username = _username }).SingleOrDefault();

                return user;
            }
        }

        public bool SaveUserNotes(string _username, string _notes)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "UPDATE  `users` SET `notes`=@notes WHERE `username`=@username;";

                return connection.Execute(sql, new { notes = _notes, username = _username }) > 0;
            }
        }

        public IEnumerable<User> FetchAll()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = string.Format("SELECT {0} FROM `users` WHERE Archived=0;", userFields);

                var users = connection.Query<User>(sql);

                if (users.HasContent())
                {
                    //var userDict = users.ToDictionary(k => k.ID, v => v);

                    sql = "SELECT a.`userId`, b.* FROM `userHasRoles` a INNER JOIN `roles` b ON a.`roleID`=b.`id` WHERE a.`userId` IN @ids;";

                    var roles = connection.Query<RoleMap>(sql, new { ids = users.Select(n => n.ID) });

                    if (roles.HasContent())
                    {
                        var rolesUserDict = new Dictionary<int, ICollection<Role>>();

                        foreach (var item in roles)
                        {
                            ICollection<Role> currentList;

                            if (!rolesUserDict.TryGetValue(item.UserId, out currentList))
                            {
                                rolesUserDict.Add(item.UserId, new Collection<Role>());
                                rolesUserDict[item.UserId].Add(new Role { ID = item.ID, Name = item.Name, Colour = item.Colour });

                            }
                            else
                            {
                                rolesUserDict[item.UserId].Add(new Role { ID = item.ID, Name = item.Name, Colour = item.Colour });
                            }
                        }

                        foreach (var user in users)
                        {
                            user.Roles = rolesUserDict[user.ID].Distinct();
                        }
                    }
                }

                return users;
            }
        }

        private class RoleMap
        {
            public int UserId { get; set; }
            public int ID { get; set; }
            public string Name { get; set; }
            public string Colour { get; set; }
        }

        public IEnumerable<User> Fetch(IEnumerable<int> _ids)
        {
            if (!_ids.HasContent())
                return null;

            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = string.Format("SELECT {0} FROM `users` WHERE `id` IN @ids  AND Archived=0;", userFields);

                var users = connection.Query<User>(sql, new { ids = _ids });

                if (users.HasContent())
                {
                   // var userDict = users.ToDictionary(k => k.ID, v => v);

                    sql = "SELECT a.`userId`, b.* FROM `userHasRoles` a INNER JOIN `roles` b ON a.`roleID`=b.`id` WHERE a.`userId` IN @ids;";

                    var roles = connection.Query<RoleMap>(sql, new {ids = users.Select(n => n.ID)});

                    if (roles.HasContent())
                    {
                        var rolesUserDict = new Dictionary<int, ICollection<Role>>();

                        foreach (var item in roles)
                        {
                            ICollection<Role> currentList;

                            if (!rolesUserDict.TryGetValue(item.UserId, out currentList))
                            {
                                rolesUserDict.Add(item.UserId, new Collection<Role>());
                                rolesUserDict[item.UserId].Add(new Role { ID = item.ID, Name = item.Name, Colour = item.Colour});
                                
                            }
                            else
                            {
                                rolesUserDict[item.UserId].Add(new Role { ID = item.ID, Name = item.Name, Colour = item.Colour });
                            }
                        }

                        foreach (var user in users)
                        {
                            user.Roles = rolesUserDict[user.ID].Distinct();
                        }
                    }
                }

                return users;
            }
        }

        public IEnumerable<User> Fetch(IEnumerable<string> _usernames)
        {
            if (!_usernames.HasContent())
                return null;

            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = string.Format("SELECT {0} FROM `users` WHERE `username` IN @usernames AND Archived=0;", userFields);

                var users = connection.Query<User>(sql, new { usernames = _usernames });

                if (users.HasContent())
                {
                    // var userDict = users.ToDictionary(k => k.ID, v => v);

                    sql = "SELECT a.`userId`, b.* FROM `userHasRoles` a INNER JOIN `roles` b ON a.`roleID`=b.`id` WHERE a.`userId` IN @ids;";

                    var roles = connection.Query<RoleMap>(sql, new { ids = users.Select(n => n.ID) });

                    if (roles.HasContent())
                    {
                        var rolesUserDict = new Dictionary<int, ICollection<Role>>();

                        foreach (var item in roles)
                        {
                            ICollection<Role> currentList;

                            if (!rolesUserDict.TryGetValue(item.UserId, out currentList))
                            {
                                rolesUserDict.Add(item.UserId, new Collection<Role>());
                                rolesUserDict[item.UserId].Add(new Role { ID = item.ID, Name = item.Name, Colour = item.Colour });

                            }
                            else
                            {
                                rolesUserDict[item.UserId].Add(new Role { ID = item.ID, Name = item.Name, Colour = item.Colour });
                            }
                        }

                        foreach (var user in users)
                        {
                            user.Roles = rolesUserDict[user.ID].Distinct();
                        }
                    }
                }

                return users;
            }
        }

        public bool Create(User _user, out int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var transactionScope = new TransactionScope())
                {
                    connection.Open();
                    var sql = "INSERT INTO `users` (`username`, `forename`, `surname`, `email`, `enabled`, `archived`) VALUES (@username, @forename, @surname, @email, @enabled, @archived);";
                    
                    var success = connection.Execute(sql, new
                        {
                            username = _user.Username,
                            forename = _user.Forename,
                            surname = _user.Surname,
                            email = _user.Email,
                            enabled = _user.Enabled,
                            archived = _user.Archived
                        }) > 0;

                    var id = connection.Query<ulong>("SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);").SingleOrDefault();
                    _id = Convert.ToInt32(id);
                    var insertObj = _user.Roles.Select(n => new { UserId = id, RoleId = n.ID });
                    sql = "INSERT INTO `UserHasRoles` (`UserID`, `RoleId`) VALUES (@UserId, @RoleId);";
                    success = connection.Execute(sql, insertObj) > 0;

                    transactionScope.Complete();  

                    return success & id > 0;
                }
            }
        }

        public bool Update(User _user)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var transactionScope = new TransactionScope())
                {
                    connection.Open();

                    var sql = "UPDATE `users` SET `username` = @username, `forename` = @forename, `surname`= @surname, `email` = @email, `enabled` = @enabled, `archived` = @archived WHERE `id` = @id";

                    var success = connection.Execute(sql, new
                        {
                            username = _user.Username,
                            forename = _user.Forename,
                            surname = _user.Surname,
                            email = _user.Email,
                            enabled = _user.Enabled,
                            archived = _user.Archived,
                            id = _user.ID
                        }) > 0;

                    //todo: how to only alter roles which have been updated?

                    sql = "SELECT `RoleID` FROM `UserHasRoles` WHERE `UserID` = @UserId";

                    var oldRoles = connection.Query<int>(sql, new { UserId = _user.ID });

                   // var enumerable = oldRoles as int[] ?? oldRoles.ToArray();
                  //  if (!enumerable.HasContent())
                  //  {
                     //   sql = "INSERT INTO `UserHasRoles` (`UserID`, `RoleId`) VALUES (@UserId, @RoleId);";
                     //   var insertObj = _user.Roles.Select(n => new { UserId = _user.ID, RoleId = n.ID });
                     //   success = connection.Execute(sql, insertObj) > 0;
                   // }
                   // else
                    // {var toDelete = enumerable.Where(n => !_user.Roles.Any(m => m.ID == n));
                    var toDelete = oldRoles.Where(n => _user.Roles.All(m => m.ID != n));
                    var toUpdate = _user.Roles.Where(n => !oldRoles.Any(m => m == n.ID));

                 //       var delete = toDelete as int[] ?? toDelete.ToArray();
                    if (toDelete.HasContent())
                        {
                            sql = "DELETE FROM `UserHasRoles` WHERE `RoleId` IN @roleIds AND `UserId` = @UserId;";

                            success = connection.Execute(sql, new { roleIds = toDelete, UserId = _user.ID }) > 0;
                        }

                        var update = toUpdate as Role[] ?? toUpdate.ToArray();
                        if (update.HasContent())
                        {
                            sql = "INSERT INTO `UserHasRoles` (`UserID`, `RoleId`) VALUES (@UserId, @RoleId);";
                            var insertObj = update.Select(n => new { UserId = _user.ID, RoleId = n.ID });
                            success = connection.Execute(sql, insertObj) > 0;
                        }
                   // }

                    transactionScope.Complete();

                    return success;
                }
            }
        }

        public bool Delete(User _user)
        {
            _user.Enabled = false;
            _user.Surname = string.Empty;
            _user.Forename = string.Empty;
            _user.Archived = false;

            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "UPDATE `users` SET `username` = @username, `forename` = @forename, `surname`= @surname, `email` = @email, `enabled` = @enabled, `archived` = @archived WHERE `id` = @id";
                var sqlParams = new
                {
                    username = _user.Username,
                    forename = string.Empty,
                    surname = string.Empty,
                    email = _user.Email,
                    enabled = false,
                    archived = true,
                    id = _user.ID
                };

                return connection.Execute(sql, sqlParams) > 0;
            }
        }

        public bool ValidateUser(string _username, string _password)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT CAST(Count(`id`) AS UNSIGNED INTEGER) FROM `users` WHERE `username`=@username AND `password`=@password;";

                return connection.Query<ulong>(sql, new { username = _username, password = _password }).SingleOrDefault() > 0;
            }
        }

        public bool SavePassword(int _id, string _salt, string _password)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "UPDATE `users` SET `salt` = @salt, `password`=@password WHERE `id`=@id;";

                var success = connection.Execute(sql, new { id = _id, salt = _salt, password = _password }) > 0;

                return success;
            }
        }

        public string GetSalt(string _username)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT `salt` FROM `users` WHERE `username`=@username;";

                return connection.Query<string>(sql, new {username = _username}).SingleOrDefault();
            }
        }

        public Profile FetchProfile(string _username)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var sql = "SELECT c.* FROM `userHasGroups` a INNER JOIN `users` b ON a.`IdUser` = b.`Id` INNER JOIN `groups` c ON a.`IdGroup` = c.`Id` WHERE `username`=@username;";

                var groups = connection.Query<Group>(sql, new { username = _username });

                sql = "SELECT * FROM `applicantreference` WHERE `PersonalID`=@username;";

                var applicantData = connection.Query<ApplicantData>(sql, new { username = _username });
                var profile = new  Profile {Groups = groups, ApplicantFields = applicantData};

                return profile;
            }
        }

        public IEnumerable<SCEData> FetchSceData(IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var sql = "SELECT * FROM `scereference` WHERE `idUser` IN @ids;";

                var sceData = connection.Query<SCEData>(sql, new{ids = _ids});

                return sceData;
            }
        }

        public SCEData FetchSceData(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
             //   connection.Open();

                var sql = "SELECT `id`, `idUser`, `ExaminationNumber`, `Title`, `SecretaryEmail`, `OtherEmail` AS 'EmailOther', `MainSpecialty`, `Trust`, `Grade`, `Address1`, `Address2`, `City`, `Region`, `Postcode`, `PhoneWork`, `PhoneMobile`, `PhoneOther`, `Trained`, `GMCNumber`, `DateTrained`  FROM `scereference` WHERE `idUser`=@id;";

                var sceData = connection.Query<SCEData>(sql, new { id = _id }).SingleOrDefault();

                return sceData;
            }
        }

        public bool Create(SCEData _sce)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                    connection.Open();
                    var sql = "INSERT INTO `scereference` (`idUser`, `ExaminationNumber`, `Title`, `SecretaryEmail`, `OtherEmail`, `MainSpecialty`, `Trust`, `Grade`, `Address1`, `Address2`, `City`, `Region`, `PostCode`, `PhoneWork`, `PhoneMobile`, `PhoneOther`, `Trained`,`GMCNumber`, `DateTrained`) VALUES (@idUser, @ExaminationNumber, @Title, @SecretaryEmail, @OtherEmail, @MainSpecialty, @Trust, @Grade, @Address1, @Address2, @City, @Region, @PostCode, @PhoneWork, @PhoneMobile, @PhoneOther, @Trained, @GMCNumber, @DateTrained);";

                    var success = connection.Execute(sql, new
                    {
                        idUser = _sce.IdUser,
                        ExaminationNumber = _sce.ExaminationNumber,
                        GMCNumber = _sce.GMCNumber,
                        Title = _sce.Title,
                        SecretaryEmail = _sce.SecretaryEmail,
                        OtherEmail = _sce.EmailOther,
                        MainSpecialty = _sce.MainSpecialty,
                        Trust = _sce.Trust,
                        Grade = _sce.Grade,
                        Address1 = _sce.Address1,
                        Address2 = _sce.Address2,
                        City = _sce.City,
                        Region = _sce.Region,
                        Postcode = _sce.Postcode,
                        PhoneWork = _sce.PhoneWork,
                        PhoneMobile = _sce.PhoneMobile,
                        PhoneOther = _sce.PhoneOther,
                        Trained = _sce.Trained,
                        DateTrained = _sce.DateTrained
                    }) > 0;

                    return success;
                
            }
        }

        public bool Update(SCEData _sce)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var sql = "Update `scereference` SET `ExaminationNumber` = @ExaminationNumber, `Title` = @Title, `SecretaryEmail` = @SecretaryEmail, `OtherEmail` = @OtherEmail, `MainSpecialty` =@MainSpecialty, `Trust` = @Trust, `Grade` = @Grade, `Address1` = @Address1, `Address2` = @Address2, `City` = @City, `Region` = @Region, `PostCode` = @PostCode, `PhoneWork` = @PhoneWork, `PhoneMobile` = @PhoneMobile, `PhoneOther` = @PhoneOther, `Trained` = @Trained,`GMCNumber` = @GMCNumber, `DateTrained`=@DateTrained WHERE `id`=@id ;";

                var success = connection.Execute(sql, new
                {
                    id = _sce.ID,
                    ExaminationNumber = _sce.ExaminationNumber,
                    GMCNumber = _sce.GMCNumber,
                    Title = _sce.Title,
                    SecretaryEmail = _sce.SecretaryEmail,
                    OtherEmail = _sce.EmailOther,
                    MainSpecialty = _sce.MainSpecialty,
                    Trust = _sce.Trust,
                    Grade = _sce.Grade,
                    Address1 = _sce.Address1,
                    Address2 = _sce.Address2,
                    City = _sce.City,
                    Region = _sce.Region,
                    Postcode = _sce.Postcode,
                    PhoneWork = _sce.PhoneWork,
                    PhoneMobile = _sce.PhoneMobile,
                    PhoneOther = _sce.PhoneOther,
                    Trained = _sce.Trained,
                    DateTrained = _sce.DateTrained
                }) > 0;

                return success;

            }
        }

        public IEnumerable<SCEData> FetchAllSceData()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var sql = "SELECT * FROM `scereference`;";

                var sceData = connection.Query<SCEData>(sql);

                return sceData;
            }
        }

        public bool UserHasWithdrawn(int userId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT CAST(Count(`id`) AS UNSIGNED INTEGER) FROM `Withdrawals` WHERE `userId`=@userId;";

                return connection.Query<ulong>(sql, new { userId = userId}).SingleOrDefault() > 0;
            }
        }

        public bool WithdrawUser(int userId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "INSERT INTO `Withdrawals` (`userId`, `DateWithdrawn`) VALUES (@userId, @dateWithdrawn);";

                return connection.Execute(sql, new { userId = userId, dateWithdrawn = DateTime.Now}) > 0;
            }
        }

        public bool AddUserToGroup(int _userId, int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "INSERT INTO `userhasgroups` (`IdGroup`,`IdUser`) VALUES (@idGroup, @idUser);";

                var success = connection.Execute(sql, new { idGroup = _id, idUser = _userId }) > 0;

                return success;
            }
        }
    }
}

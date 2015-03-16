using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Dapper;
using Extensions;
using MySql.Data.MySqlClient;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects.Signups;

namespace eMotive.Repository.Objects
{
    public class MySqlSessionRepository : ISessionRepository
    {
        private readonly string connectionString;

        public MySqlSessionRepository(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public IEnumerable<Group> FetchGroups()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                return connection.Query<Group>("SELECT * FROM `groups`;");
            }
        }

        public IEnumerable<Group> FetchGroups(IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                return connection.Query<Group>("SELECT * FROM `groups` WHERE `id` IN @ids;", new { ids = _ids });
            }
        }

        /*public IEnumerable<UserSignupProjection> FetchSignupProjectionsForUser(int _userId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                return connection.Query<UserSignupProjection>("SELECT a.`ID`, a.`Date`, a.`Description`  FROM `signup` a INNER JOIN `slot` b ON a.`ID` = b.`IdSignUp` INNER JOIN `userhasslots` c ON b.`ID` = c.`IdSlot` WHERE c.`IdUser` = @id", new { id = _userId });
            }
        }*/

        public IEnumerable<Signup> FetchSignupsByGroup(IEnumerable<int> _groupIds)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                //Fetch all signup information which is assigned to any of the passed _groupIds
                var sql = "SELECT * FROM `signup` a INNER JOIN `groups` b ON a.`idGroup` = b.`id` WHERE `idGroup` IN  @ids;";
                var signUps = connection.Query<Signup, Group, Signup>(sql, (signupDTO, groupDTO) => { signupDTO.Group = groupDTO; return signupDTO; }, new { ids = _groupIds });

                if (signUps.HasContent())
                {
                    //get all signupIds
                    var qIds = signUps.Select(n => n.id);

                    //select all slots which belong to the passed in signup ids
                    sql = "SELECT * FROM `slot` WHERE `idSignUp` in @ids;";
                    var slots = connection.Query<Slot>(sql, new { ids = qIds });

                    if (slots.HasContent())
                    {
                        //get all slot ids
                        qIds = slots.Select(n => n.id);

                        //pull out any applicants assigned to any of the slot ids we have passed in
                       // sql = "SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate`FROM `UserHasSlots` a INNER JOIN `users` b ON a.id=b.id WHERE a.`idSlot` IN @ids;";
                        sql = "SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate`FROM `UserHasSlots` a INNER JOIN `users` b ON a.idUser=b.id WHERE a.`idSlot` IN @ids ORDER BY `SignUpDate`, `idSlot` DESC;";
                        var userSignups = connection.Query<UserSignup>(sql, new { ids = qIds });

                        if (userSignups.HasContent())
                        {
                            //build a dictionary of userSignups against slot id
                            var signDict = userSignups.GroupBy(k => k.IdSlot, v => v).ToDictionary(k => k.Key, v => v.ToList());

                            //loop through slots and add all userSignups to a slot if a match is found
                            foreach (var slot in slots)
                            {
                                foreach (var userSignup in signDict)
                                {
                                    if (slot.id == userSignup.Key)
                                    {
                                        slot.UsersSignedUp = userSignup.Value;

                                        var i = 0;
                                        foreach (var user in slot.UsersSignedUp)
                                        {
                                            i++;
                                            if (i <= slot.PlacesAvailable)
                                            {
                                                continue;
                                            }

                                            user.Type = i <= (slot.PlacesAvailable + slot.ReservePlaces) ? SlotType.Reserve : SlotType.Interested;

                                        }

                                    }

                                }
                            }
                        }

                        //build a dictionary of slots against signupid
                        var slotDict = slots.GroupBy(k => k.IdSignUp, v => v).ToDictionary(k => k.Key, v => v.ToList());

                        //loop through signups and add all slots to a signup if a match is found
                        foreach (var si in signUps)
                        {
                            foreach (var slot in slotDict)
                            {
                                if (si.id == slot.Key)
                                    si.Slots = slot.Value;
                            }
                        }
                    }
                }

                return signUps;

            }
        }

        public IEnumerable<Signup> FetchSignups(IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                //Fetch all signup information which is assigned to any of the passed _groupIds
                var sql = "SELECT * FROM `signup` a INNER JOIN `groups` b ON a.`idGroup` = b.`id` WHERE a.`id` IN  @ids;";
                var signUps = connection.Query<Signup, Group, Signup>(sql, (signupDTO, groupDTO) => { signupDTO.Group = groupDTO; return signupDTO; }, new { ids = _ids });

                if (signUps.HasContent())
                {
                    //get all signupIds
                    var qIds = signUps.Select(n => n.id);

                    //select all slots which belong to the passed in signup ids
                    sql = "SELECT * FROM `slot` WHERE `idSignUp` in @ids;";
                    var slots = connection.Query<Slot>(sql, new { ids = qIds });

                    if (slots.HasContent())
                    {
                        //get all slot ids
                        qIds = slots.Select(n => n.id);

                        //pull out any applicants assigned to any of the slot ids we have passed in
                        // sql = "SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate`FROM `UserHasSlots` a INNER JOIN `users` b ON a.id=b.id WHERE a.`idSlot` IN @ids;";
                        sql = "SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate`FROM `UserHasSlots` a INNER JOIN `users` b ON a.idUser=b.id WHERE a.`idSlot` IN @ids ORDER BY `SignUpDate`, `idSlot` DESC;";
                        var userSignups = connection.Query<UserSignup>(sql, new { ids = qIds });

                        if (userSignups.HasContent())
                        {
                            //build a dictionary of userSignups against slot id
                            var signDict = userSignups.GroupBy(k => k.IdSlot, v => v).ToDictionary(k => k.Key, v => v.ToList());

                            //loop through slots and add all userSignups to a slot if a match is found
                            foreach (var slot in slots)
                            {
                                foreach (var userSignup in signDict)
                                {
                                    if (slot.id == userSignup.Key)
                                    {
                                        slot.UsersSignedUp = userSignup.Value;

                                        var i = 0;
                                        foreach (var user in slot.UsersSignedUp)
                                        {
                                            i++;
                                            if (i <= slot.PlacesAvailable)
                                            {
                                                continue;
                                            }

                                            user.Type = i <= (slot.PlacesAvailable + slot.ReservePlaces) ? SlotType.Reserve : SlotType.Interested;

                                        }

                                    }

                                }
                            }
                        }

                        //build a dictionary of slots against signupid
                        var slotDict = slots.GroupBy(k => k.IdSignUp, v => v).ToDictionary(k => k.Key, v => v.ToList());

                        //loop through signups and add all slots to a signup if a match is found
                        foreach (var si in signUps)
                        {
                            foreach (var slot in slotDict)
                            {
                                if (si.id == slot.Key)
                                    si.Slots = slot.Value;
                            }
                        }
                    }
                }

                return signUps;

            }
        }

        public IEnumerable<Signup> FetchSignupsForUser(int _userId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                //Fetch all signup information which is assigned to any of the passed _groupIds
                var sql = "SELECT * FROM `Signup` a INNER JOIN `groups` b ON a.`idGroup` = b.`id` WHERE a.`id` IN (SELECT `idSignUp` FROM slot c INNER JOIN `UserHasSlots` d ON c.`id` = d.`idSlot` WHERE d.`idUser`=@userId);";
                var signUps = connection.Query<Signup, Group, Signup>(sql, (signupDTO, groupDTO) => { signupDTO.Group = groupDTO; return signupDTO; }, new { userId = _userId });

                if (signUps.HasContent())
                {
                    //get all signupIds
                    var qIds = signUps.Select(n => n.id);

                    //select all slots which belong to the passed in signup ids
                    sql = "SELECT * FROM `slot` WHERE `idSignUp` in @ids;";
                    var slots = connection.Query<Slot>(sql, new { ids = qIds });

                    if (slots.HasContent())
                    {
                        //get all slot ids
                        qIds = slots.Select(n => n.id);

                        //pull out any applicants assigned to any of the slot ids we have passed in
                        // sql = "SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate`FROM `UserHasSlots` a INNER JOIN `users` b ON a.id=b.id WHERE a.`idSlot` IN @ids;";
                        sql = "SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate`FROM `UserHasSlots` a INNER JOIN `users` b ON a.idUser=b.id WHERE a.`idSlot` IN @ids ORDER BY `SignUpDate`, `idSlot` DESC;";
                        var userSignups = connection.Query<UserSignup>(sql, new { ids = qIds });

                        if (userSignups.HasContent())
                        {
                            //build a dictionary of userSignups against slot id
                            var signDict = userSignups.GroupBy(k => k.IdSlot, v => v).ToDictionary(k => k.Key, v => v.ToList());

                            //loop through slots and add all userSignups to a slot if a match is found
                            foreach (var slot in slots)
                            {
                                foreach (var userSignup in signDict)
                                {
                                    if (slot.id == userSignup.Key)
                                    {
                                        slot.UsersSignedUp = userSignup.Value;

                                        var i = 0;
                                        foreach (var user in slot.UsersSignedUp)
                                        {
                                            i++;
                                            if (i <= slot.PlacesAvailable)
                                            {
                                                continue;
                                            }

                                            user.Type = i <= (slot.PlacesAvailable + slot.ReservePlaces) ? SlotType.Reserve : SlotType.Interested;

                                        }

                                    }

                                }
                            }
                        }

                        //build a dictionary of slots against signupid
                        var slotDict = slots.GroupBy(k => k.IdSignUp, v => v).ToDictionary(k => k.Key, v => v.ToList());

                        //loop through signups and add all slots to a signup if a match is found
                        foreach (var si in signUps)
                        {
                            foreach (var slot in slotDict)
                            {
                                if (si.id == slot.Key)
                                    si.Slots = slot.Value;
                            }
                        }
                    }
                }

                return signUps;

            }
        }

        public IEnumerable<Signup> FetchAllTraining()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                //Fetch all signup information which is assigned to any of the passed _groupIds
                var sql = "SELECT * FROM `signup` a INNER JOIN `groups` b ON a.`idGroup` = b.`id` WHERE `IsTraining` = true;";
                var signUps = connection.Query<Signup, Group, Signup>(sql, (signupDTO, groupDTO) => { signupDTO.Group = groupDTO; return signupDTO; });

                if (signUps.HasContent())
                {
                    //get all signupIds
                    var qIds = signUps.Select(n => n.id);

                    //select all slots which belong to the passed in signup ids
                    sql = "SELECT * FROM `slot` WHERE `idSignUp` in @ids;";
                    var slots = connection.Query<Slot>(sql, new { ids = qIds });

                    if (slots.HasContent())
                    {
                        //get all slot ids
                        qIds = slots.Select(n => n.id);

                        //pull out any applicants assigned to any of the slot ids we have passed in
                        // sql = "SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate`FROM `UserHasSlots` a INNER JOIN `users` b ON a.id=b.id WHERE a.`idSlot` IN @ids;";
                        sql = "SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate`FROM `UserHasSlots` a INNER JOIN `users` b ON a.idUser=b.id WHERE a.`idSlot` IN @ids ORDER BY `SignUpDate`, `idSlot` DESC;";
                        var userSignups = connection.Query<UserSignup>(sql, new { ids = qIds });

                        if (userSignups.HasContent())
                        {
                            //build a dictionary of userSignups against slot id
                            var signDict = userSignups.GroupBy(k => k.IdSlot, v => v).ToDictionary(k => k.Key, v => v.ToList());

                            //loop through slots and add all userSignups to a slot if a match is found
                            foreach (var slot in slots)
                            {
                                foreach (var userSignup in signDict)
                                {
                                    if (slot.id == userSignup.Key)
                                    {
                                        slot.UsersSignedUp = userSignup.Value;

                                        var i = 0;
                                        foreach (var user in slot.UsersSignedUp)
                                        {
                                            i++;
                                            if (i <= slot.PlacesAvailable)
                                            {
                                                continue;
                                            }

                                            user.Type = i <= (slot.PlacesAvailable + slot.ReservePlaces) ? SlotType.Reserve : SlotType.Interested;

                                        }

                                    }

                                }
                            }
                        }

                        //build a dictionary of slots against signupid
                        var slotDict = slots.GroupBy(k => k.IdSignUp, v => v).ToDictionary(k => k.Key, v => v.ToList());

                        //loop through signups and add all slots to a signup if a match is found
                        foreach (var si in signUps)
                        {
                            foreach (var slot in slotDict)
                            {
                                if (si.id == slot.Key)
                                    si.Slots = slot.Value;
                            }
                        }
                    }
                }

                return signUps;

            }
        }

        public IEnumerable<Signup> FetchAll()
        {
            var groups = FetchGroups();

            if (!groups.HasContent())
                return null;

            return FetchSignupsByGroup(groups.Select(n => n.ID));
        }

        public bool RegisterAttendanceToSession(SessionAttendance _session)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var transaction = new TransactionScope())
                {
                    connection.Open();

                    var alreadyExists = connection.Query<SessionAttendance>("SELECT * FROM `sessionattendance` WHERE `SessionId` = @sessionId AND UserID = @UserID;", new { sessionID = _session.SessionID, UserID = _session.UserID }).SingleOrDefault();
                    bool success = false;

                    if (alreadyExists == null)
                    {
                        success = connection.Execute(
                            "INSERT INTO `sessionattendance` (`SessionId`, `UserID`, `DateRecorded`) VALUES (@sessionID, @UserID, @DateRecorded)",
                            new
                            {
                                sessionID = _session.SessionID,
                                UserID = _session.UserID,
                                DateRecorded = DateTime.Now
                            }) > 0;
                    }
                    else
                    {
                        success = connection.Execute("UPDATE `sessionattendance` SET `SessionId` = @SessionId, `UserID` = @UserID, `DateRecorded` = @DateRecorded WHERE `id`=@id;",
                            new
                            {
                                sessionID = _session.SessionID,
                                UserID = _session.UserID,
                                DateRecorded = DateTime.Now,
                                id = alreadyExists.ID
                            }) > 0;
                    }

                    transaction.Complete();

                    return success;
                }
            }
        }

        public Signup Fetch(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                //Fetch the signup which has the passed id
                var sql = "SELECT * FROM `signup` a INNER JOIN `groups` b ON a.`idGroup` = b.`id` WHERE a.`id` = @id;";
                var signUp = connection.Query<Signup, Group, Signup>(sql, (signupDTO, groupDTO) => { signupDTO.Group = groupDTO; return signupDTO; }, new { id = _id }).SingleOrDefault();

                if (signUp != null)
                {

                    //select all slots which belong to the passed in signup ids
                    sql = "SELECT * FROM `slot` WHERE `idSignUp` = @id;";
                    var slots = connection.Query<Slot>(sql, new { id = signUp.id });

                    if (slots.HasContent())
                    {
                        //get all slot ids
                        var qIds = slots.Select(n => n.id);

                        //pull out any applicants assigned to any of the slot ids we have passed in
                        //    sql = string.Format("SELECT * FROM `{0}`.`applicant_has_slots` WHERE `idSlot` IN @ids;", DatabaseName);

                        sql = @"SELECT a.`id`, a.`idslot`, a.`idUser`, a.`SignUpDate` FROM `UserHasSlots` a INNER JOIN `users` b ON a.idUser=b.id WHERE a.`idSlot` IN @ids ORDER BY `SignUpDate`, `idSlot` DESC;";

                        var userSignups = connection.Query<UserSignup>(sql, new { ids = qIds });

                        if (userSignups.HasContent())
                        {
                            //build a dictionary of userSignups against slot id
                            var signDict = userSignups.GroupBy(k => k.IdSlot, v => v).ToDictionary(k => k.Key, v => v.ToList());

                            //loop through slots and add all userSignups to a slot if a match is found
                            foreach (var slot in slots)
                            {
                                foreach (var userSignup in signDict)
                                {
                                    if (slot.id == userSignup.Key)
                                    {
                                        slot.UsersSignedUp = userSignup.Value;

                                        var i = 0;
                                        foreach(var user in slot.UsersSignedUp)
                                        {
                                            i++;
                                            if (i <= slot.PlacesAvailable)
                                            {
                                                continue;
                                            }

                                            user.Type = i <= (slot.PlacesAvailable + slot.ReservePlaces) ? SlotType.Reserve : SlotType.Interested;
                                            
                                        }

                                    }

                                }
                            }
                        }

                        signUp.Slots = slots;

                    }
                }

                return signUp;

            }
        }

        public bool Save(Signup _signup)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var success = false;
                using (var transactionScope = new TransactionScope())
                {
                    connection.Open();

                    var query = "UPDATE `signup` SET `Date`=@Date, `idGroup`=@idGroup,`AcademicYear`=@AcademicYear,`CloseDate`=@CloseDate,`Closed`=@Closed,`OverrideClose`=@OverrideClose,`MergeReserve`=@MergeReserve,`AllowMultipleSignups`=@AllowMultipleSignups,`Description`=@Description,`IsTraining`=@IsTraining, `PlacementTags`=@PlacementTags, `DetailedPlacementText`=@DetailedPlacementText WHERE `id`=@id;";

                    if (connection.Execute(query, _signup) > 0)
                    {
                        if (!_signup.Slots.HasContent())
                        {
                            success = true;
                        }
                        else
                        {//todo: time! perhaps add duration?
                            query = "UPDATE `slot` SET `Description`=@Description,`PlacesAvailable`=@PlacesAvailable,`Enabled`=@Enabled,`ReservePlaces`=@ReservePlaces,`InterestedPlaces`=@InterestedPlaces WHERE `id`=@id;";

                            success = connection.Execute(query, _signup.Slots) > 0;
                        }
                    }

                    transactionScope.Complete();
                    return success;
                }
            }
        }

        public int GetSignupIdFromSlot(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string query = "SELECT CAST(`idSignUp` AS UNSIGNED INTEGER) FROM `slot` WHERE `id`=@idSlot;";

                return Convert.ToInt32(connection.Query<ulong>(query, new { idSlot = _id }).Single());
            }
        }

        public bool CreateGroup(Group _group)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string query = "INSERT INTO `groups` (`Name`, `AllowMultipleSignups`, `Description`, `EnableEmails`,`AllowSelfSignup`, `SelfSignupDeniedMessage`) VALUES (@name, @allowMultipleSignups, @description, @EnableEmails, @allowSelfSignup, @selfSignupDeniedMessage);";

                return connection.Execute(query, new
                {
                    name = _group.Name,
                    allowMultipleSignups = _group.AllowMultipleSignups,
                    description = _group.Description
                }) > 0;
            }
        }

        public bool UpdateGroup(Group _group)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string query = "UPDATE `groups` SET `Name`=@name, `AllowMultipleSignups`=@allowMultipleSignups, `Description`=@description, `EnableEmails`=@enableEmails, `AllowSelfSignup`=@allowSelfSignup, `SelfSignupDeniedMessage`=@selfSignupDeniedMessage WHERE `Id`=@id;";

                return connection.Execute(query, _group) > 0;
            }
        }

        public Group FetchSignupGroup(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT b.`Id`, b.`Name`, b.`AllowMultipleSignups`, b.`Description`, b.`AllowSelfSignup`, b.`SelfSignupDeniedMessage` FROM `signup` a INNER JOIN `groups` b ON a.`idGroup`=b.`id` WHERE a.`id`=@signupId";

                return connection.Query<Group>(sql, new { signupId = _id }).SingleOrDefault();
            }
        }

        public int FetchRCPActivityCode(int _signupId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "SELECT `Code` FROM `RCPActivityCodes` WHERE `idSignup`=@idSignup;";

                return connection.Query<int>(sql, new { idSignup = _signupId }).SingleOrDefault();
            }
        }

        public UserSignup FetchUserSignup(int _userId, int _groupId)
        {
            throw new NotImplementedException();
        }

        /* public UserSignup FetchUserSignup(int _userId, int _groupId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT a.*, b.`Description`, b.`idSignUp` FROM `userhasslots` a INNER JOIN `slot` b ON a.`idSlot` = b.`id` WHERE idSlot IN (SELECT `id` FROM `slot` WHERE idSignUp IN (SELECT `id` FROM `signup` WHERE idGroup=@idGroup)) AND idUser=@idUser";

                return connection.Query<UserSignup>(sql, new { idGroup = _groupId, idUser = _userId }).SingleOrDefault();
            }
        }*/

        public UserSignup FetchUserSignup(int _userId, IEnumerable<int> _groupIds)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT a.`id`, b.`id` AS 'IdSlot', c.`id` AS 'IdSignUp', c.`Date`, c.`Description`, a.`SignUpDate` FROM `userhasslots` a INNER JOIN `slot` b ON a.`idSlot` = b.`id` INNER JOIN `signup` c ON b.`idSignUp` = c.`id` WHERE idSlot IN (SELECT `id` FROM `slot` WHERE idSignUp IN (SELECT `id` FROM `signup` WHERE idGroup IN @idGroups)) AND idUser=@idUser";

                return connection.Query<UserSignup>(sql, new { idGroups = _groupIds, idUser = _userId }).SingleOrDefault();
            }
        }

        public IEnumerable<UserSignup> FetchUserSignups(int _userId, IEnumerable<int> _groupIds)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT a.`id`, b.`id` AS 'IdSlot', c.`id` AS 'IdSignUp', c.`Date`, c.`Description`, a.`SignUpDate`  FROM `userhasslots` a INNER JOIN `slot` b ON a.`idSlot` = b.`id` INNER JOIN `signup` c ON b.`idSignUp` = c.`id` WHERE idSlot IN (SELECT `id` FROM `slot` WHERE idSignUp IN (SELECT `id` FROM `signup` WHERE idGroup IN @idGroups)) AND idUser=@idUser";

                return connection.Query<UserSignup>(sql, new { idGroups = _groupIds, idUser = _userId });
            }
        }

        public bool WillingToChangeSignup(WillingToChangeSignup change)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var transaction = new TransactionScope())
                {
                    var success = true;

                    connection.Open();

                    var sql = "SELECT CAST(COUNT(*) AS UNSIGNED INTEGER)  FROM `WillingToChangeSignup` WHERE `UserID`=@UserID AND `SignupID`=@SignupID;";
                    var count = Convert.ToInt32(connection.Query<ulong>(sql, change).SingleOrDefault());
                    if (count > 0)
                    {
                        sql = "DELETE FROM `WillingToChangeSignup` WHERE `UserID`=@UserID AND `SignupID`=@SignupID;";

                        success &= connection.Execute(sql, change) > 0;
                        
                    }
                    else
                    {
                        sql = "INSERT INTO `WillingToChangeSignup` (`UserID`, `SignupID`) VALUES (@UserID, @SignupID)";
                        success &= connection.Execute(sql, change) > 0;
                    }

                    transaction.Complete();
                    return success;
                }
            }
        }

        public IEnumerable<WillingToChangeSignup> FetchWillingToChangeForSignup(int signupID)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "SELECT * FROM `WillingToChangeSignup` WHERE `SignupID`=@SignupID;";

                return connection.Query<WillingToChangeSignup>(sql, new {SignupID = signupID});
            }
        }

        public IEnumerable<WillingToChangeSignup> FetchWillingToChangeForUser(int userID)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "SELECT * FROM `WillingToChangeSignup` WHERE `UserID`=@UserID;";

                return connection.Query<WillingToChangeSignup>(sql, new { UserID = userID });
            }
        }

        public bool SignupToSlot(int _idSlot, int _idUser, DateTime _signupDate, out int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var query = "SELECT CAST(COUNT(*) AS UNSIGNED INTEGER) FROM `userhasslots` WHERE `idSlot`=@idSlot AND `idUser`=@idUser;";
                //user has already signed up for this slot! We should never get here, but this is a failsafe
                if (Convert.ToInt32(connection.Query<ulong>(query, new { idSlot = _idSlot, idUser = _idUser }).SingleOrDefault()) > 0)
                {
                    _id = -1;
                    return false;
                }

                query = "INSERT INTO `userhasslots` (`idSlot`,`idUser`,`SignUpDate`) VALUES (@idSlot, @idUser, @SignupDate);";

                var rowsAffected = connection.Execute(query, new { idSlot = _idSlot, idUser = _idUser, SignupDate = _signupDate });

                if (rowsAffected > 0)
                {
                    query = "SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);";

                    var id = connection.Query<ulong>(query).Single();

                    if (id > 0)
                    {
                        _id = Convert.ToInt32(id);
                        return true;
                    }
                }

                _id = -1;
                return false;
            }
        }

        public bool CancelSignupToSlot(int _idSlot, int _idUser)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string query = "DELETE FROM `userhasslots` WHERE `idSlot`=@idSlot AND `idUser`=@idUser;";

                var rowsAffected = connection.Execute(query, new { idSlot = _idSlot, idUser = _idUser });

                return rowsAffected > 0;
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
        /*                    var id = connection.Query<ulong>("SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);").SingleOrDefault();
                    _id = Convert.ToInt32(id);
                    var insertObj = _user.Roles.Select(n => new { UserId = id, RoleId = n.ID });
                    sql = "INSERT INTO `UserHasRoles` (`UserID`, `RoleId`) VALUES (@UserId, @RoleId);";*/
        public bool AddUserToGroups(int _userId, IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "INSERT INTO `userhasgroups` (`IdGroup`,`IdUser`) VALUES (@idGroup, @idUser);";
                var insertObj = _ids.Select(n => new { idUser = _userId, idGroup = n });
                var success = connection.Execute(sql, insertObj) > 0;

                return success;
            }     
        }

        public bool UpdateUsersGroups(int _userId, IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var transaction = new TransactionScope())
                {
                    connection.Open();

                    var sql = "DELETE FROM `userhasgroups` WHERE `IdUser` = @idUser;";
                    var success = connection.Execute(sql, new { idUser = _userId }) > 0;

                    sql = "INSERT INTO `userhasgroups` (`IdGroup`,`IdUser`) VALUES (@idGroup, @idUser);";
                    var insertObj = _ids.Select(n => new {idUser = _userId, idGroup = n});
                    success &= connection.Execute(sql, insertObj) > 0;

                    transaction.Complete();

                    return success;
                }
            }   
        }
    }
}

using System.Collections.Generic;
using Dapper;
using eMotive.Models.Objects.Reports.Users;
using MySql.Data.MySqlClient;
using eMotive.Services.Interfaces;

namespace eMotive.Services.Objects
{
    public class ReportService : IReportService
    {
        private readonly string connectionString;

        public ReportService(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public IEnumerable<SCEReportItem> FetchAllSCEs()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql =
                    "SELECT a.*, c.* FROM `Users` a INNER JOIN `UserhasRoles` b ON a.id=b.UserID INNER JOIN `SceReference` c ON a.`ID`=c.`idUser` WHERE b.`RoleID`=6 AND a.`Archived` = 0;";
                return connection.Query<SCEReportItem>(sql);
            }
        }

        public IEnumerable<SCEReportItem> FetchUsersNotSignedUp()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                //const string sql = "SELECT a.* FROM `users` a INNER JOIN `userhasroles` b ON a.`ID` = b.`UserId` WHERE b.RoleID=4 AND `ID` NOT IN (SELECT `IdUser` FROM `userhasslots`);";
                const string sql =
                    "SELECT a.*, c.* FROM `Users` a INNER JOIN `UserhasRoles` b ON a.id=b.UserID INNER JOIN `SceReference` c ON a.`ID`=c.`idUser` WHERE b.`RoleID`=6;";
                    //"SELECT a.*, c.* FROM `Users` a INNER JOIN `UserhasRoles` b ON a.id=b.UserID INNER JOIN `SceReference` c ON a.`ID`=c.`idUser` WHERE b.`RoleID`=6 AND a.ID NOT IN (SELECT idUser FROM `UserHasSlots` WHERE `idSlot` NOT IN (221,222,223,224));";
                return connection.Query<SCEReportItem>(sql);
            }
        }

        public IEnumerable<SCEReportItem> FetchSCEData(IEnumerable<int> _userIds)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "SELECT a.`ID` AS 'UserID',a.*, b.* FROM `Users` a INNER JOIN `SceReference` b ON a.`ID`=b.`idUser` WHERE a.`ID` IN @ids;";

                return connection.Query<SCEReportItem>(sql, new {ids = _userIds});
            }
        }

        public IEnumerable<InterviewerReportItem> FetchAllInterviewers()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql =
                    "SELECT users.*, interviewerReference.* FROM `Users` users INNER JOIN `UserhasRoles` userHasRoles ON users.id=userHasRoles.UserID INNER JOIN `Roles` roles ON userHasRoles.`RoleId` = roles.`ID` INNER JOIN `scereference` interviewerreference ON users.`ID`=interviewerReference.`idUser` INNER JOIN `userhasgroups` userHasGroups ON users.`ID`= userHasGroups.`IdUser` INNER JOIN `Groups` groups ON userHasGroups.`IdGroup` = groups.`ID` WHERE roles.`Name`='Interviewer' AND (groups.Name='Interviewer' AND groups.Name!='Observer');";
                   // "SELECT a.*, c.* FROM `Users` a INNER JOIN `UserhasRoles` b ON a.id=b.UserID INNER JOIN `InterviewerReference` c ON a.`ID`=c.`idUser` WHERE b.`RoleID`=6;";

                return connection.Query<InterviewerReportItem>(sql);
            }
        }
        public IEnumerable<SCEReportItem> FetchSCEsNotSignedUp()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                //const string sql = "SELECT a.* FROM `users` a INNER JOIN `userhasroles` b ON a.`ID` = b.`UserId` WHERE b.RoleID=4 AND `ID` NOT IN (SELECT `IdUser` FROM `userhasslots`);";
                const string sql =
                    "SELECT users.*, interviewerReference.* FROM `Users` users INNER JOIN `UserhasRoles` userHasRoles ON users.id=userHasRoles.UserID INNER JOIN `Roles` roles ON userHasRoles.`RoleId` = roles.`ID` INNER JOIN `scereference` interviewerreference ON users.`ID`=interviewerReference.`idUser` WHERE roles.`Name`='SCE' AND users.ID NOT IN (SELECT idUser FROM `UserHasSlots`);";

                return connection.Query<SCEReportItem>(sql);
            }
        }


        public IEnumerable<InterviewerReportItem> FetchInterviewersNotSignedUp()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                //const string sql = "SELECT a.* FROM `users` a INNER JOIN `userhasroles` b ON a.`ID` = b.`UserId` WHERE b.RoleID=4 AND `ID` NOT IN (SELECT `IdUser` FROM `userhasslots`);";
                const string sql =
                    "SELECT users.*, interviewerReference.* FROM `Users` users INNER JOIN `UserhasRoles` userHasRoles ON users.id=userHasRoles.UserID INNER JOIN `Roles` roles ON userHasRoles.`RoleId` = roles.`ID` INNER JOIN `scereference` interviewerreference ON users.`ID`=interviewerReference.`idUser` WHERE roles.`Name`='SCE' AND users.ID NOT IN (SELECT idUser FROM `UserHasSlots`);";
                
                return connection.Query<InterviewerReportItem>(sql);
            }
        }

        public IEnumerable<InterviewerReportItem> FetchAllObservers()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql =
                    "SELECT users.*, interviewerReference.* FROM `Users` users INNER JOIN `UserhasRoles` userHasRoles ON users.id=userHasRoles.UserID INNER JOIN `Roles` roles ON userHasRoles.`RoleId` = roles.`ID` INNER JOIN `SceReference` interviewerreference ON users.`ID`=interviewerReference.`idUser` INNER JOIN `userhasgroups` userHasGroups ON users.`ID`= userHasGroups.`IdUser` INNER JOIN `Groups` groups ON userHasGroups.`IdGroup` = groups.`ID` WHERE roles.`Name`='Interviewer' AND (groups.Name!='Interviewer' AND groups.Name='Observer');";

                return connection.Query<InterviewerReportItem>(sql);
            }
        }

        public IEnumerable<InterviewerReportItem> FetchObserversNotSignedUp()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                //const string sql = "SELECT a.* FROM `users` a INNER JOIN `userhasroles` b ON a.`ID` = b.`UserId` WHERE b.RoleID=4 AND `ID` NOT IN (SELECT `IdUser` FROM `userhasslots`);";
                const string sql =
                    "SELECT users.*, interviewerReference.* FROM `Users` users INNER JOIN `UserhasRoles` userHasRoles ON users.id=userHasRoles.UserID INNER JOIN `Roles` roles ON userHasRoles.`RoleId` = roles.`ID` INNER JOIN `scereference` interviewerreference ON users.`ID`=interviewerReference.`idUser` INNER JOIN `userhasgroups` userHasGroups ON users.`ID`= userHasGroups.`IdUser` INNER JOIN `Groups` groups ON userHasGroups.`IdGroup` = groups.`ID` WHERE roles.`Name`='Interviewer' AND (groups.Name!='Interviewer' AND groups.Name='Observer') AND users.ID NOT IN (SELECT idUser FROM `UserHasSlots`);";

                return connection.Query<InterviewerReportItem>(sql);
            }
        }

        public IEnumerable<InterviewerReportItem> FetchAllInterviewersAndObservers()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql =
                    "SELECT users.*, interviewerReference.*, GROUP_CONCAT(groups.Name) AS 'Groups' FROM `Users` users INNER JOIN `UserhasRoles` userHasRoles ON users.id=userHasRoles.UserID INNER JOIN `Roles` roles ON userHasRoles.`RoleId` = roles.`ID` INNER JOIN `scereference` interviewerreference ON users.`ID`=interviewerReference.`idUser` INNER JOIN `userhasgroups` userHasGroups ON users.`ID`= userHasGroups.`IdUser` INNER JOIN `Groups` groups ON userHasGroups.`IdGroup` = groups.`ID` WHERE roles.`Name`='Interviewer' GROUP BY users.ID;";

                return connection.Query<InterviewerReportItem>(sql);
            }
        }

        public IEnumerable<InterviewerReportItem> FetchInterviewersAndObserversNotSignedUp()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql =
                    "SELECT users.*, interviewerReference.*, GROUP_CONCAT(groups.Name) AS 'Groups' FROM `Users` users INNER JOIN `UserhasRoles` userHasRoles ON users.id=userHasRoles.UserID INNER JOIN `Roles` roles ON userHasRoles.`RoleId` = roles.`ID` INNER JOIN `scereference` interviewerreference ON users.`ID`=interviewerReference.`idUser` INNER JOIN `userhasgroups` userHasGroups ON users.`ID`= userHasGroups.`IdUser` INNER JOIN `Groups` groups ON userHasGroups.`IdGroup` = groups.`ID` WHERE roles.`Name`='Interviewer' AND users.ID NOT IN (SELECT idUser FROM `UserHasSlots`) GROUP BY users.ID;";

                return connection.Query<InterviewerReportItem>(sql);
            }
        }

        public IEnumerable<WillingToChangeSignup> FetchWillingToChangeSignups()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "SELECT * FROM `WillingToChangeSignup`;";
                return connection.Query<WillingToChangeSignup>(sql);
            }
        }
    }
}

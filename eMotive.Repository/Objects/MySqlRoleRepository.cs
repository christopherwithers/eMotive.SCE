using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects.Users;

namespace eMotive.Repository.Objects
{
    public class MySqlRoleRepository : IRoleRepository
    {
        private readonly string connectionString;

        public MySqlRoleRepository(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public Role New()
        {
            return new Role();
        }

        public Role Fetch(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "SELECT `Id`, `Name`, `Colour` FROM `roles` WHERE `id`=@id;";

                return connection.Query<Role>(sql, new { id = _id }).SingleOrDefault();
            }
        }

        public Role Fetch(string _name)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "SELECT `Id`, `Name`, `Colour` FROM `roles` WHERE `Name`=@name;";

                return connection.Query<Role>(sql, new { name = _name }).SingleOrDefault();
            }
        }

        public IEnumerable<Role> Fetch(IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "SELECT `Id`, `Name`, `Colour` FROM `roles` WHERE `id` IN @ids;";

                return connection.Query<Role>(sql, new { ids = _ids });
            }
        }

        public IEnumerable<Role> Fetch(IEnumerable<string> _names)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "SELECT `Id`, `Name`, `Colour` FROM `roles` WHERE `Name` IN @names;";

                return connection.Query<Role>(sql, new { names = _names });
            }
        }

        public IEnumerable<Role> FetchAll()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "SELECT `Id`, `Name`, `Colour` FROM `roles`;";

                return connection.Query<Role>(sql);
            }
        }

        //http://stackoverflow.com/questions/6387904/how-to-insert-an-ienumerablet-collection-with-dapper-dot-net
        public bool AddUserToRoles(int _id, IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "INSERT INTO `UserHasRoles` (`UserId`, `RoleId`) VALUES (@idUser, @idRole);";

                return connection.Execute(sql, new {Enumerable = _ids.Select(n => new {idUser = _id, idRole = n}) }) > 0;
            }
        }

        public bool RemoveUserFromRoles(int _userId, IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "DELETE FROM `UserHasRoles` WHERE `UserId` = @idUser AND `RoleId` = @idRole);";

                return connection.Execute(sql, new { Enumerable = _ids.Select(n => new { idUser = _userId, idRole = n }) }) > 0;
            }
        }

        public IEnumerable<int> FindUsersInRole(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "SELECT `UserId` FROM `UserHasRoles` WHERE `RoleId` = @id;";

                return connection.Query<int>(sql, new { id = _id});
            }
        }

        public IEnumerable<Role> FetchUserRoles(int _userId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "SELECT a.`id`, a.`Name`, a.`Colour` FROM `roles` a INNER JOIN `UserHasRoles` b ON a.`id`=b.`idRole` WHERE b.`idUser` = @idUser;";

                return connection.Query<Role>(sql, new { idUser = _userId });
            }
        }

        public bool Update(Role _role)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "UPDATE `roles` SET `Name`= @name, `Colour`=@colour WHERE `id` = @id;";

                return connection.Execute(sql, new { name = _role.Name, id = _role.ID , colour = _role.Colour}) > 0;
            }
        }

        public bool Create(Role _role)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "INSERT INTO `roles` (`Name`, `Colour`) VALUES (@name, @colour);";

                return connection.Execute(sql, new { name = _role.Name, colour = _role.Colour }) > 0;
            }
        }

        public bool Delete(string _role)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "DELETE FROM `roles WHERE `Name`=@name;";

                return connection.Execute(sql, new { name = _role }) > 0;
            }
        }

        public bool Delete(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "DELETE FROM `roles` WHERE `id`=@id;";

                return connection.Execute(sql, new { id = _id }) > 0;
            }
        }
    }
}

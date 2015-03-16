using System.Collections.Generic;
using eMotive.Repository.Objects.Users;

namespace eMotive.Repository.Interfaces
{
    public interface IRoleRepository
    {
        Role New();

        Role Fetch(int _id);
        Role Fetch(string _name);

        IEnumerable<Role> Fetch(IEnumerable<int> _ids);
        IEnumerable<Role> Fetch(IEnumerable<string> _names);

        IEnumerable<Role> FetchAll(); 

        bool AddUserToRoles(int _id, IEnumerable<int> _ids);
        bool RemoveUserFromRoles(int _userId, IEnumerable<int> _ids);
        IEnumerable<int> FindUsersInRole(int _id);

        IEnumerable<Role> FetchUserRoles(int _userId); 
        bool Update(Role _role);
        bool Create(Role _role);
        bool Delete(string _role);
        bool Delete(int _id);
    }
}
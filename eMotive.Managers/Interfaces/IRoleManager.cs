using System.Collections.Generic;
using eMotive.Models.Objects.Roles;
using eMotive.Search.Interfaces;
using modSearch = eMotive.Models.Objects.Search;

namespace eMotive.Managers.Interfaces
{
    public interface IRoleManager : ISearchable
    {
        Role New();
        Role Fetch(int _id);
        Role Fetch(string _name);

        bool Create(Role _role, out int _id);
        bool Update(Role _role);
        bool Delete(Role _role);
        IEnumerable<Role> FetchAll();
    }
}
/*        Role New();

        Role Fetch(int _id);
        Role Fetch(string _name);

        IEnumerable<Role> Fetch(IEnumerable<int> _ids);
        IEnumerable<Role> Fetch(IEnumerable<string> _names);

        bool AddUserToRoles(int _id, IEnumerable<int> _ids);
        bool RemoveUserFromRoles(int _userId, IEnumerable<int> _ids);
        IEnumerable<int> FindUsersInRole(int _id);

        IEnumerable<Role> FetchUserRoles(int _userId); 
        bool Update(Role _role);
        bool Create(string _role);
        bool Delete(string _role);
        bool Delete(int _id);*/
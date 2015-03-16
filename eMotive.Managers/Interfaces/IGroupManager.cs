using System.Collections.Generic;
using eMotive.Models.Objects.Signups;

namespace eMotive.Managers.Interfaces
{
    public interface IGroupManager
    {
        bool AddUserToGroups(int _userId, IEnumerable<int> _ids);
        bool UpdateUsersGroups(int _userId, IEnumerable<int> _ids);
        bool AddUserToGroup(int _userId, int _id);
        IEnumerable<Group> FetchGroups();
        Group FetchGroup(int _id);
        bool CreateGroup(Group _group);
        bool UpdateGroup(Group _group);
    }
}

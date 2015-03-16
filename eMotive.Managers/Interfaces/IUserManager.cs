using System.Collections.Generic;
using eMotive.Models.Objects.Users;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;
using repUsers = eMotive.Repository.Objects.Users;
using modSearch = eMotive.Models.Objects.Search;

namespace eMotive.Managers.Interfaces
{
    public interface IUserManager : ISearchable
    {
        User New();
        User Fetch(int _id);
        IEnumerable<User> FetchAll();
        User Fetch(string _username);

        string FetchUserNotes(string _username);
        bool SaveUserNotes(string _username, string _notes);

        IEnumerable<User> Fetch(IEnumerable<int> _ids);
        IEnumerable<User> Fetch(IEnumerable<string> _usernames);


        bool Create(User _user, out int _id);
        bool Create(User _user, IEnumerable<int> _groupIds);
        bool Update(User _user);
        bool Delete(User _user);
        IEnumerable<User> FetchRecordsFromSearch(SearchResult _searchResult);


        bool CreateSCE(SCEData _sce, out int _id);
        bool UpdateSCE(SCEData _sce);

        Profile FetchProfile(string _username);

        SCEData FetchSCEData(int _id);
        IEnumerable<SCEData> FetchAllSceData();
    }
}

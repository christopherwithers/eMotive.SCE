using System.Collections.Generic;
using eMotive.Repository.Objects.Users;

namespace eMotive.Repository.Interfaces
{
    //TODO: NEED TO CREATE A PROFILE CLASS WHICH PULLS IN USER GROUP AND OTHER NEEDED INFO!
    public interface IUserRepository
    {
        User New();
        User Fetch(int _id);
        User Fetch(string _value, UserField _field);

        string FetUserNotes(string _username);
        bool SaveUserNotes(string _username, string _notes);

        IEnumerable<User> FetchAll();
        IEnumerable<User> Fetch(IEnumerable<int> _ids);
        IEnumerable<User> Fetch(IEnumerable<string> _usernames);

        bool Create(User _user, out int _id);
        bool Update(User _user);
        bool Delete(User _user);
        
        bool ValidateUser(string _username, string _password);

        bool SavePassword(int _id, string _salt, string _password);
        string GetSalt(string _username);

        Profile FetchProfile(string _username);

        IEnumerable<SCEData> FetchSceData(IEnumerable<int> _ids);
        SCEData FetchSceData(int _id);
        bool Create(SCEData _sce);
        bool Update(SCEData _sce);
        IEnumerable<SCEData> FetchAllSceData();

        bool UserHasWithdrawn(int userId);
        bool WithdrawUser(int userId);

        
    }
}

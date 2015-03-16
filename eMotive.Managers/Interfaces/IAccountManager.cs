using eMotive.Models.Objects.Account;
using eMotive.Models.Objects.Users;

namespace eMotive.Managers.Interfaces
{
    public interface IAccountManager
    {
        bool ValidateUser(string _username, string _password);
        bool CreateNewAccountPassword(User _user);
        bool ResendPassword(string _email);
        bool ResendUsername(string _email);
        bool ChangePassword(ChangePassword _changePassword);
        bool ResendAccountCreationEmail(string _username);

        bool WithdrawUser(int _userID);
        bool UserHasWithdrawn(int userId);
    }
}

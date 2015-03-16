using System;
using System.Collections.Generic;
using eMotive.Repository.Objects.Signups;

namespace eMotive.Repository.Interfaces
{
    public interface ISessionRepository
    {
        IEnumerable<Group> FetchGroups();
        IEnumerable<Group> FetchGroups(IEnumerable<int> _ids);
        IEnumerable<Signup> FetchSignupsByGroup(IEnumerable<int> _groupIds);
        IEnumerable<Signup> FetchSignups(IEnumerable<int> _ids);

        IEnumerable<Signup> FetchSignupsForUser(int _userId);

        IEnumerable<Signup> FetchAllTraining();
        IEnumerable<Signup> FetchAll();
        Signup Fetch(int _id);

        bool RegisterAttendanceToSession(SessionAttendance _session);

        bool AddUserToGroup(int _userId, int _id);
        bool AddUserToGroups(int _userId, IEnumerable<int> _ids);
        bool UpdateUsersGroups(int _userId, IEnumerable<int> _ids);
        //todo: fetch signups for user!

        bool SignupToSlot(int _idSlot, int _idUser, DateTime _signupDate, out int _id);
        bool CancelSignupToSlot(int _idSlot, int _idUser);

        bool Save(Signup _signup);

        int GetSignupIdFromSlot(int _id);

        bool CreateGroup(Group _group);
        bool UpdateGroup(Group _group);
        Group FetchSignupGroup(int _id);

        int FetchRCPActivityCode(int _signupId);

        //TODO: HAVE A SIGNUPINFO CLASS FOR USERS WHO HAVE SIGNED UP?? COULD CONTAIN IDSIGNUP, IDSLOT ETC

        UserSignup FetchUserSignup(int _userId, int _groupId);
        UserSignup FetchUserSignup(int _userId, IEnumerable<int> _groupIds);
        IEnumerable<UserSignup> FetchUserSignups(int _userId, IEnumerable<int> _groupIds);

        bool WillingToChangeSignup(WillingToChangeSignup change);
        IEnumerable<WillingToChangeSignup> FetchWillingToChangeForSignup(int signupID);
        IEnumerable<WillingToChangeSignup> FetchWillingToChangeForUser(int userID);
        //  IEnumerable<UserSignupProjection> FetchSignupProjectionsForUser(int _userId);

    }
}

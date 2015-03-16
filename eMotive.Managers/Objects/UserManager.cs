using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using eMotive.Models.Objects.Roles;
using Extensions;
using Lucene.Net.Search;
using eMotive.Managers.Interfaces;
using eMotive.Managers.Objects.Search;
using eMotive.Models.Objects;
using eMotive.Models.Objects.Search;
using eMotive.Models.Objects.Users;
using eMotive.Repository;
using eMotive.Repository.Interfaces;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;
using eMotive.Services.Interfaces;
using Profile = eMotive.Models.Objects.Users.Profile;
using repUsers = eMotive.Repository.Objects.Users;
using emSearch = eMotive.Search.Objects.Search;

namespace eMotive.Managers.Objects
{
    public class UserManager : IUserManager
    {

        private readonly IUserRepository userRep;
        private readonly ISearchManager searchManager;
        private readonly IAccountManager accountManager;
        private readonly IGroupManager groupManager;
        
        public UserManager(IUserRepository _userRep, IAccountManager _accountManager, 
            ISearchManager _searchManager, IGroupManager _groupManager)
        {
            userRep = _userRep;
            accountManager = _accountManager;
            searchManager = _searchManager;
            groupManager = _groupManager;

            AutoMapperManagerConfiguration.Configure();
        }

        public INotificationService notificationService { get; set; }

        public IRoleManager roleManager { get; set; }

        public User New()
        {
            return Mapper.Map<repUsers.User, User>(userRep.New());
        }

        public User Fetch(int _id)
        {
            var repUser = userRep.Fetch(_id);

            return Mapper.Map<repUsers.User, User>(repUser);
        }

        public IEnumerable<User> FetchAll()
        {
            return Mapper.Map<IEnumerable<repUsers.User>, IEnumerable<User>>(userRep.FetchAll());
        }

        public User Fetch(string _username)
        {
            var repUser = userRep.Fetch(_username, UserField.Username);

            return Mapper.Map<repUsers.User, User>(repUser);
        }

        public string FetchUserNotes(string _username)
        {
            return userRep.FetUserNotes(_username);
        }

        public bool SaveUserNotes(string _username, string _notes)
        {
            return userRep.SaveUserNotes(_username, _notes);
        }

        public IEnumerable<User> Fetch(IEnumerable<int> _ids)
        {
            return Mapper.Map<IEnumerable<repUsers.User>, IEnumerable<User>>(userRep.Fetch(_ids));
        }

        public IEnumerable<User> Fetch(IEnumerable<string> _usernames)
        {
            return Mapper.Map<IEnumerable<repUsers.User>, IEnumerable<User>>(userRep.Fetch(_usernames));
        }

        public bool Create(User _user, IEnumerable<int> _groupIds)
        {
            var user = Mapper.Map<User, repUsers.User>(_user);

            var checkUser = userRep.Fetch(_user.Username, UserField.Username);

            if (checkUser != null)
            {
                if (checkUser.Archived)
                {
                    notificationService.AddIssue(GetErrorMessage(CreateUser.Deletedaccount, _user.Username));
                    return false;
                }

                if (_user.Username.ToLowerInvariant() == checkUser.Username.ToLowerInvariant())
                {
                    notificationService.AddIssue(GetErrorMessage(CreateUser.DuplicateUsername, _user.Username));
                    return false;
                }
            }

            checkUser = userRep.Fetch(_user.Email, UserField.Email);

            if (checkUser != null)
            {
                if (_user.Email.ToLowerInvariant() == checkUser.Email.ToLowerInvariant())
                {
                    notificationService.AddIssue(GetErrorMessage(CreateUser.DuplicateEmail, _user.Email));
                    return false;
                }
            }

            int id;
            if (userRep.Create(user, out id))
            {
                _user.ID = id;
                groupManager.AddUserToGroups(_user.ID, _groupIds);//TODO: ALTER THIS #################################################################################
                if (accountManager.CreateNewAccountPassword(_user))
                {
                    _user.ID = user.ID = id;
                    searchManager.Add(new UserSearchDocument(user));

                    return true;
                }
                return false;
            }

            notificationService.AddError(GetErrorMessage(CreateUser.Error));
            return false;
        }

        //TODO: if we cache, then assign id to _user too and cache Models.User
        public bool Create(User _user, out int _id)
        {
            var user = Mapper.Map<User, repUsers.User>(_user);
            
            var checkUser = userRep.Fetch(_user.Username, UserField.Username);

            if (checkUser != null)
            {
                if (checkUser.Archived)
                {
                    notificationService.AddIssue(GetErrorMessage(CreateUser.Deletedaccount, _user.Username));
                    _id = -1;
                    return false;
                }

                if (_user.Username.ToLowerInvariant() == checkUser.Username.ToLowerInvariant())
                {
                    notificationService.AddIssue(GetErrorMessage(CreateUser.DuplicateUsername, _user.Username));
                    _id = -1;
                    return false;
                }
            }

            checkUser = userRep.Fetch(_user.Email, UserField.Email);

            if (checkUser != null)
            {
                if (_user.Email.ToLowerInvariant() == checkUser.Email.ToLowerInvariant())
                {
                    notificationService.AddIssue(GetErrorMessage(CreateUser.DuplicateEmail, _user.Email));
                    _id = -1;
                    return false;
                }
            }

            int id;
            if (userRep.Create(user, out id))
            {
                _user.ID = id;
                groupManager.AddUserToGroup(_user.ID, 1);//TODO: ALTER THIS #################################################################################
                if (accountManager.CreateNewAccountPassword(_user))
                {
                    _user.ID = user.ID = id;
                    searchManager.Add(new UserSearchDocument(user));

                    _id = id;
                    return true;
                }
                _id = id;
                return false;
            }

            notificationService.AddError(GetErrorMessage(CreateUser.Error));
            _id = -1;
            return false;
        }

        public bool CreateSCE(SCEData _sce, out int _id)
        {
            var role = roleManager.FetchAll().SingleOrDefault( n => n.Name == "SCE");
            var tempUser = new UserWithRoles
            {
                Username = _sce.Username,
                Forename = _sce.Forename,
                Surname = _sce.Surname,
                Email = _sce.Email,
                Enabled = _sce.Enabled,
                Archived = false,
                Roles = new[] {role}
            };

            var _user = Mapper.Map<User, repUsers.User>(tempUser);

            var checkUser = userRep.Fetch(_user.Username, UserField.Username);

            if (checkUser != null)
            {
                if (checkUser.Archived)
                {
                    notificationService.AddIssue(GetErrorMessage(CreateUser.Deletedaccount, _user.Username));
                    _id = -1;
                    return false;
                }

                if (_user.Username.ToLowerInvariant() == checkUser.Username.ToLowerInvariant())
                {
                    notificationService.AddIssue(GetErrorMessage(CreateUser.DuplicateUsername, _user.Username));
                    _id = -1;
                    return false;
                }
            }

            checkUser = userRep.Fetch(_user.Email, UserField.Email);

            if (checkUser != null)
            {
                if (_user.Email.ToLowerInvariant() == checkUser.Email.ToLowerInvariant())
                {
                    notificationService.AddIssue(GetErrorMessage(CreateUser.DuplicateEmail, _user.Email));
                    _id = -1;
                    return false;
                }
            }

            int id;
            if (userRep.Create(_user, out id))
            {
                _user.ID = tempUser.ID = id;
                _id = id;
                groupManager.AddUserToGroups(_user.ID, _sce.BelongsToGroups.Select(int.Parse));
                _sce.IdUser = id;
                
                    if (userRep.Create(Mapper.Map<SCEData, repUsers.SCEData>(_sce)))
                {
                    if (accountManager.CreateNewAccountPassword(tempUser))
                    {
                        searchManager.Add(new UserSearchDocument(_user));
                        return true;
                    }
                }

                return false;
            }

            notificationService.AddError(GetErrorMessage(CreateUser.Error));
            _id = -1;
            return false;
        }

        public bool UpdateSCE(SCEData _sce)
        {
            var role = roleManager.FetchAll().SingleOrDefault(n => n.Name == "SCE");
            var tempUser = new UserWithRoles
            {
                ID = _sce.IdUser,
                Username = _sce.Username,
                Forename = _sce.Forename,
                Surname = _sce.Surname,
                Email = _sce.Email,
                Enabled = _sce.Enabled,
                Archived = false,
                Roles = new[] { role }
            };

            var _user = Mapper.Map<User, repUsers.User>(tempUser);

            var checkUser = userRep.Fetch(_user.Username, UserField.Username);

            if (checkUser != null)
            {
                if (_user.Username.ToLowerInvariant() == checkUser.Username.ToLowerInvariant() && _sce.IdUser != checkUser.ID)
                {
                    notificationService.AddIssue(string.Format("A user with the username '{0}' already exists.", _user.Username));
                    return false;
                }
            }

            checkUser = userRep.Fetch(_user.Email, UserField.Email);

            if (checkUser != null)
            {
                if (_user.Email.ToLowerInvariant() == checkUser.Email.ToLowerInvariant() && _user.ID != checkUser.ID)
                {
                    notificationService.AddIssue(string.Format("A user with the email address '{0}' already exists.", _user.Email));
                    return false;
                }
            }

            var user = Mapper.Map<User, repUsers.User>(tempUser);

            if (userRep.Update(user))
            {
                groupManager.UpdateUsersGroups(_sce.IdUser, _sce.BelongsToGroups.Select(int.Parse));

                if (userRep.Update(Mapper.Map<SCEData, repUsers.SCEData>(_sce)))
                {
                    searchManager.Update(new UserSearchDocument(user));
                }
                return true;
            }

            notificationService.AddError("An error occurred while trying to edit the user. An administrator has been notified of this issue.");

            return false;
        }

        public bool Update(User _user)
        {
            var checkUser = userRep.Fetch(_user.Username, UserField.Username);

            if (checkUser != null)
            {
                if (_user.Username.ToLowerInvariant() == checkUser.Username.ToLowerInvariant() && _user.ID != checkUser.ID)
                {
                    notificationService.AddIssue(string.Format("A user with the username '{0}' already exists.", _user.Username));
                    return false;
                }
            }

            checkUser = userRep.Fetch(_user.Email, UserField.Email);

            if (checkUser != null)
            {
                if (_user.Email.ToLowerInvariant() == checkUser.Email.ToLowerInvariant() && _user.ID != checkUser.ID)
                {
                    notificationService.AddIssue(string.Format("A user with the email address '{0}' already exists.", _user.Email));
                    return false;
                }
            } 
            
            var user = Mapper.Map<User, repUsers.User>(_user);

            if (userRep.Update(user))
            {
                searchManager.Update(new UserSearchDocument(user));
                return true;
            }

            notificationService.AddError("An error occurred while trying to edit the user. An administrator has been notified of this issue.");

            return false;
        }

        public bool Delete(User _user)
        {
            var user = Mapper.Map<User, repUsers.User>(_user);

            if (userRep.Delete(user))
            {
                var deletedUser = userRep.Fetch(_user.ID);
                searchManager.Update(new UserSearchDocument(deletedUser));

                return true;
            }

            notificationService.AddError(string.Format("An error occured while trying to delete '{0}'. An administrator has been notified of this issue.", _user.Username));

            return false;
        }

        public IEnumerable<User> FetchRecordsFromSearch(SearchResult _searchResult)
        {
            if (_searchResult.Items.HasContent())
            {
                var repItems = userRep.Fetch(_searchResult.Items.Select(n => n.ID).ToList());
                if (repItems.HasContent())
                {
                    return Mapper.Map<IEnumerable<repUsers.User>, IEnumerable<User>>(repItems);

                }
            }

            return null;
        }

        public Profile FetchProfile(string _username)
        {
            return Mapper.Map<repUsers.Profile, Profile>(userRep.FetchProfile(_username));
        }

        public SCEData FetchSCEData(int _id)
        {
            return Mapper.Map<repUsers.SCEData, SCEData>(userRep.FetchSceData(_id));
        }

        public IEnumerable<SCEData> FetchAllSceData()
        {
            return Mapper.Map <IEnumerable<repUsers.SCEData>, IEnumerable<SCEData>>(userRep.FetchAllSceData());
        }


        public SearchResult DoSearch(BasicSearch _search)
        {
            var newSearch = Mapper.Map<BasicSearch, emSearch>(_search);

            if (_search.Filter.HasContent())
            {
                foreach (var filter in _search.Filter)
                {
                    newSearch.Filters.Add(filter.Key, new emSearch.SearchTerm { Field = filter.Value, Term = Occur.MUST });
                }
            }

            if (!string.IsNullOrEmpty(newSearch.Query))
            {
                newSearch.CustomQuery = new Dictionary<string, emSearch.SearchTerm>
                {
                    {"Username", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Forename", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Surname", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Email", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"GMCNumber", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                };
         
            }

            return searchManager.DoSearch(newSearch);

        }

        public void ReindexSearchRecords()
        {
            var records = userRep.FetchAll();
            var users = records.Where(n => n.Roles.Any(m => m.Name != "SCE"));
            var sces = records.Where(n => n.Roles.Any(m => m.Name == "SCE"));

            if (!users.HasContent())
            {
                //todo: send an error message here
                return;
            }

            foreach (var item in users)
            {
                searchManager.Add(new UserSearchDocument(item));
            }

            if (!sces.HasContent())
            {
                //todo: send an error message here
                return;
            }

            var sceData = userRep.FetchSceData(sces.Select(n => n.ID));
            var userDict = sces.ToDictionary(k => k.ID, v => v);

            foreach (var data in sceData)
            {
                data.Username = userDict[data.IdUser].Username;
                data.Forename = userDict[data.IdUser].Forename;
                data.Surname = userDict[data.IdUser].Surname;
                data.Email = userDict[data.IdUser].Email;
                data.Archived = userDict[data.IdUser].Archived;
            }

            foreach (var item in sceData)
            {
                searchManager.Add(new SCESearchDocument(item));
            }
        }

        private static string GetErrorMessage(CreateUser _message, string _field = "")
        {
            switch (_message)
            {
                    case CreateUser.Deletedaccount:
                        return string.Format("The account for username '{0}' has been deleted.", _field);
                    case CreateUser.DuplicateUsername:
                        return string.Format("The username '{0}' is unavailable.", _field);
                    case CreateUser.DuplicateEmail:
                        return string.Format("The email address '{0}' is already registered to an account.", _field);
                    case CreateUser.Error:
                        goto default;
                default:
                    return "An error occured. An administrator has been notified of this issue.";
            }
        }
    }
}

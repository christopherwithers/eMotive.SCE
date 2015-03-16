using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Extensions;
using eMotive.Managers.Interfaces;
using eMotive.Managers.Objects.Search;
using eMotive.Models.Objects.Roles;
using eMotive.Models.Objects.Search;
using eMotive.Repository.Interfaces;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;
using eMotive.Services.Interfaces;
using rep = eMotive.Repository.Objects.Users;
namespace eMotive.Managers.Objects
{
    public class RoleManager : IRoleManager
    {
        private readonly IRoleRepository roleRepository;
        private readonly ISearchManager searchManager;

        public RoleManager(IRoleRepository _roleRepository, ISearchManager _searchManager)
        {
            roleRepository = _roleRepository;
            searchManager = _searchManager;

            AutoMapperManagerConfiguration.Configure();
        }

        public INotificationService notificationService { get; set; }

        public Role New()
        {
            return Mapper.Map<rep.Role, Role>(roleRepository.New());
        }

        public Role Fetch(int _id)
        {
            throw new System.NotImplementedException();
        }

        public Role Fetch(string _name)
        {
            throw new System.NotImplementedException();
        }

        public bool Create(Role _role, out int _id)
        {
            var role = Mapper.Map<Role, rep.Role>(_role);

            var checkRole = roleRepository.Fetch(role.Name);

            if (checkRole != null)
            {
                if (_role.Name.ToLowerInvariant() == checkRole.Name.ToLowerInvariant())
                {
                    notificationService.AddIssue(string.Format("A role with the name '{0}' already exists.", _role.Name));
                    _id = -1;
                    return false;
                }
            }

            if (roleRepository.Create(role))
            {
                var newRole = roleRepository.Fetch(_role.Name);
                searchManager.Add(new RoleSearchDocument(newRole));
                _id = newRole.ID;
                return true;
            }

            notificationService.AddError("An error occured while trying to create the role. An administrator has been notified of this issue.");
            _id = -1;
            return false;
        }

        public bool Update(Role _role)
        {
            var checkRole = roleRepository.Fetch(_role.Name);

            if (checkRole != null)
            {
                if (_role.Name.ToLowerInvariant() == checkRole.Name.ToLowerInvariant() && _role.ID != checkRole.ID)
                {
                    notificationService.AddIssue(string.Format("A role with the name '{0}' already exists.", _role.Name));
                    return false;
                }
            }
            var role = Mapper.Map<Role, rep.Role>(_role);

            if (roleRepository.Update(role))
            {
                searchManager.Update(new RoleSearchDocument(role));
                return true;
            }

            notificationService.AddError("An error occured while trying to edit the role. An administrator has been notified of this issue.");

            return false;
        }

        public bool Delete(Role _role)
        {
            var role = Mapper.Map<Role, rep.Role>(_role);

            var roleEmpty = roleRepository.FindUsersInRole(role.ID);

            if (roleEmpty.HasContent())
            {
                notificationService.AddIssue(string.Format("'{0}' is assigned to {1} users. The role could not be deleted.", _role.Name, roleEmpty.Count()));
                return false;
            }

            if (roleRepository.Delete(_role.ID))
            {
                searchManager.Delete(new RoleSearchDocument(role));
                return true;
            }

            notificationService.AddError("An error occured while trying to edit the role. An administrator has been notified of this issue.");

            return false;
        }

        public IEnumerable<Role> FetchAll()
        {
            return Mapper.Map<IEnumerable<rep.Role>, IEnumerable<Role>>(roleRepository.FetchAll());
        }

        public SearchResult DoSearch(BasicSearch _search)
        {
        //    return searchManager.DoSearch(_search);
            throw new System.NotImplementedException();
        }

        public void ReindexSearchRecords()
        {
            var records = roleRepository.FetchAll();

            if (!records.HasContent())
            {
                //todo: send an error message here
                return;
            }

            foreach (var item in records)
            {
                searchManager.Add(new RoleSearchDocument(item));
            }
        }
    }
}

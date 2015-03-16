using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Signups;
using eMotive.Repository.Interfaces;
using eMotive.Services.Interfaces;


namespace eMotive.Managers.Objects
{
    //TODO add search stuff??
    public class GroupManager : IGroupManager
    {
        private readonly ISessionRepository signupRepository;

        public GroupManager(ISessionRepository _signupRepository)
        {
            signupRepository = _signupRepository;
        }

        public INotificationService notificationService { get; set; }

        public bool UpdateUsersGroups(int _userId, IEnumerable<int> _ids)
        {
            return signupRepository.UpdateUsersGroups(_userId, _ids);
        }

        public bool AddUserToGroup(int _userId, int _id)
        {
            return signupRepository.AddUserToGroup(_userId, _id);
        }

        public IEnumerable<Group> FetchGroups()
        {
            return Mapper.Map <IEnumerable<Repository.Objects.Signups.Group>, IEnumerable<Group>>(signupRepository.FetchGroups());
        }

        //TODO: this needs to pull single group from signupRep, bit of a bodge atm
        public Group FetchGroup(int _id)
        {
            var group = Mapper.Map<IEnumerable<Repository.Objects.Signups.Group>, IEnumerable<Group>>(signupRepository.FetchGroups(new[] { _id }));

            return group.FirstOrDefault();
        }

        public bool CreateGroup(Group _group)
        {
            return signupRepository.CreateGroup(Mapper.Map<Group, Repository.Objects.Signups.Group>(_group));
        }

        public bool UpdateGroup(Group _group)
        {
            return signupRepository.UpdateGroup(Mapper.Map<Group, Repository.Objects.Signups.Group>(_group));
        }

        public bool AddUserToGroups(int _userId, IEnumerable<int> _ids)
        {
            return signupRepository.AddUserToGroups(_userId, _ids);
        }
    }
}

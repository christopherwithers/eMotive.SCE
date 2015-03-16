using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects.Users;

namespace eMotive.Core.Tests
{
    //http://www.codeproject.com/Articles/5019/Advanced-Unit-Testing-Part-I-Overview
    //http://www.peterprovost.org/blog/2012/04/15/visual-studio-11-fakes-part-1
    [TestClass]
    public class UserRepositoryTest
    {
        
        private IUserRepository userRepository;

        [TestInitialize]
        public void Initialize()
        {
            var UserRepository = new Mock<IUserRepository>();
            UserRepository.Setup(n => n.New()).Returns(new User());
            UserRepository.Setup(n => n.Fetch(It.IsAny<int>())).Returns(new User {ID = 1, Username = "ted"});
            userRepository = UserRepository.Object;
        }

        [TestMethod]
        public void NewReturnsNotNull()
        {
            var user = userRepository.New();
            Assert.IsNotNull(user);

        }

        [TestMethod]
        public void ConstructorInitialization()
        {
            var user = userRepository.New();

            Assert.AreEqual(user.ID, 0);
            Assert.AreEqual(user.Username, null);
            Assert.AreEqual(user.Forename, null);
            Assert.AreEqual(user.Surname, null);
            Assert.AreEqual(user.Email, null);
            Assert.AreEqual(user.Archived, false);
            Assert.AreEqual(user.Enabled, false);

        }

        [TestMethod]
        public void UsernameIsAssignable()
        {
            var user = userRepository.New();

            user.Username = "ted";
            Assert.AreEqual(user.Username, "ted");
        }

        [TestCleanup]
        public void Terminate()
        {
        }
    }
}

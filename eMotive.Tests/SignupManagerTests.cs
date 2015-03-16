using System;
using System.Collections.ObjectModel;
using eMotive.Managers.Interfaces;
using eMotive.Managers.Objects;
using eMotive.Models.Objects.Roles;
using eMotive.Models.Objects.Users;
using eMotive.Repository;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects.Signups;
using eMotive.Services.Interfaces;
using NUnit.Framework;
/* (look into Selenium)
 * ################################################
 * ############# NAMING CONVENTION ################
 * ################################################
 * # MethodName_StateUnderTest_ ExpectedBehaviour #
 * ################################################
 */
using Rhino.Mocks;

namespace eMotive.Tests
{
    [TestFixture]
    public class SignupManagerTests
    {
        private ISessionRepository signupRepository;
        private IUserManager userManager;
        private IEmailService emailService;
        private INotificationService notificationService;
        private IeMotiveConfigurationService configurationService;

        private ISessionManager signupManager;

        [SetUp]
        public void Setup()
        {
            signupRepository = MockRepository.GenerateStub<ISessionRepository>();
            userManager = MockRepository.GenerateStub<IUserManager>();
            emailService = MockRepository.GenerateStub<IEmailService>();
            notificationService = MockRepository.GenerateStub<INotificationService>();
            configurationService = MockRepository.GenerateStub<IeMotiveConfigurationService>();


            #region Signup Repository Setup
            signupRepository.Stub(n => n.Fetch(1)).Return(new Signup
            {
                id = 1,
                AcademicYear = "1314",
                AllowMultipleSignups = true,
                Closed = false,
                CloseDate = DateTime.MaxValue,
                Date = DateTime.Now,
                Group = new Group { AllowMultipleSignups = true, DisabilitySignups = false, ID = 1, Name = "Group 1" },
                MergeReserve = true,
                OverrideClose = false,
                Slots = new[]
                {
                    new Slot
                    {
                        Description = "Slot 1",
                        Enabled = true,
                        id = 1,
                        IdSignUp = 1,
                        InterestedPlaces = 5,
                        ReservePlaces = 5,
                        PlacesAvailable = 5,
                        Time = DateTime.Now,
                        UsersSignedUp =
                            new Collection<UserSignup>
                            {
                                new UserSignup
                                {
                                    Description = "Slot 1",
                                    ID = 1,
                                    IdSignUp = 1,
                                    IdSlot = 1,
                                    IdUser = 1,
                                    SignUpDate = DateTime.Now,
                                    Type = SlotType.Main
                                }
                            }
                    },
                    new Slot
                    {
                        Description = "Slot 2",
                        Enabled = true,
                        id = 2,
                        IdSignUp = 1,
                        InterestedPlaces = 5,
                        ReservePlaces = 5,
                        PlacesAvailable = 5,
                        Time = DateTime.Now,
                        UsersSignedUp = null
                    }
                }
            });
            #endregion
            #region UserManager Setup

            userManager.Stub(n => n.Fetch(1))
                .Return(new User
                {
                    Archived = false,
                    Email = "user1@bham.ac.uk",
                    Enabled = true,
                    Forename = "Forename",
                    Surname = "Surname",
                    ID = 1,
                    RoleString = string.Empty,
                    Roles = new Collection<Role> { new Role { Colour = "FFFFFF", ID = 1, Name = "Role 1" } },
                    Username = "User1"
                });
               userManager.Stub(n => n.Fetch(new int[] {})).IgnoreArguments().Return(new[] { new User
                   {
                       Archived = false,
                       Email = "user1@bham.ac.uk",
                       Enabled = true,
                       Forename = "Forename",
                       Surname = "Surname",
                       ID = 1,
                       RoleString = string.Empty,
                       Roles = new Collection<Role> {new Role {Colour = "FFFFFF", ID = 1, Name = "Role 1"}},
                       Username = "User1"
                   }
           });

        //    userManager.Stub(n => n.Fetch(coll))

            #endregion
            #region NotificationService Setup
            //notificationService.Stub(c => c.AddError())
            #endregion

            var mocks = new MockRepository();
            signupManager = mocks.StrictMock<SessionManager>(signupRepository, userManager);

        }

        [Test]
        public void Fetch_ExistingSignupID_ReturnsSignup()
        {
            var signup = signupManager.Fetch(1);

            Assert.AreEqual(1, signup.ID);
        }

        [Test]
        public void Fetch_SignupIDDoesntExist_ReturnsNull()
        {
            var signup = signupManager.Fetch(3);

            Assert.IsNull(signup);
        }
        /*
        [Test]
        public void Fetch_ExistingSignupID_ReturnsSignup()
        {
            signupRepository.Stub(x => x.FetchSignup(1)).Return(new Signup {id = 1});

            var signup = signupRepository.FetchSignup(1);

            Assert.AreEqual(1, signup.id);
        }

        [Test]
        public void Fetch_ExistingSignupID_ReturnsSignupWithNoUsersSignedUp()
        {
            signupRepository.Stub(x => x.FetchSignup(1)).Return(new Signup { id = 1 });

            var signup = signupRepository.FetchSignup(1);

            Assert.AreEqual(1, signup.id);
        }

        [Test]
        public void Fetch_ExistingSignupID_ReturnsSignupWithUserSignedUp()
        {
            signupRepository.Stub(x => x.FetchSignup(1)).Return(new Signup { id = 1 });
            signupRepository.Stub(x => x.FetchSignup(1)).Return(new Signup 
            { 
                id = 1, 
                Slots = new Collection<Slot>
            {
                new Slot
                {
                    id = 1,
                    new UserSignup
                    {
                        ID = 1,

                    }
                }}
            }
            )
            var signup = signupRepository.FetchSignup(1);

            Assert.AreEqual(1, signup.id);
        }

        [Test]
        public void Fetch_SignupIDDoesntExist_ReturnsNull()
        {
            var signup = signupRepository.FetchSignup(1);

            Assert.IsNull(signup);
        }

        [Test]
        public void Fetch_NegativeSignupID_ReturnsNull()
        {
            var signup = signupRepository.FetchSignup(-1);

            Assert.IsNull(signup);
        }*/


        [TearDown]
        public void TearDown()
        {
            signupRepository = null;
            userManager = null;
            emailService = null;
            notificationService = null;
            configurationService = null;
        }
    }
}



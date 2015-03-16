using System.Linq;
using eMotive.Services;
using eMotive.Services.Interfaces;
using NUnit.Framework;
/* (look into Selenium)
 * ################################################
 * ############# NAMING CONVENTION ################
 * ################################################
 * # MethodName_StateUnderTest_ ExpectedBehaviour #
 * ################################################
 */
namespace eMotive.Tests
{
    [TestFixture]
    public class NotificationServiceTests
    {
        public INotificationService notificationService;

        [SetUp]
        public void Setup()
        {
           // notificationService = new NotificationService();
        }

      /*  [Test]
        public void FetchErrors_UpperCaseLoggingDisabledAddError_ReturnsNull()
        {
            notificationService = new NotificationService("ConnectionString", "false");
            notificationService.AddError("A new error");

            Assert.IsNull(notificationService.Messages.Count > ());
        }*/
        
        [Test]
        public void FetchErrors_LowerCaseLoggingDisabledAddError_ReturnsNull()
        {
            notificationService = new NotificationService("ConnectionString", "FALSE");
            notificationService.AddError("A new error");

            Assert.IsNull(notificationService.FetchErrors());
        }

        [Test]
        public void FetchErrors_LoggingEnabledAddErrorFetchErrors_ReturnsError()
        {
            notificationService = new NotificationService("ConnectionString", "true");
            notificationService.AddError("A new error");
            Assert.AreEqual(new []{"A new error"}, notificationService.FetchErrors());
        }

        [Test]
        public void FetchErrors_UpperCaseLoggingDisabledAddIssue_ReturnsNull()
        {
            notificationService = new NotificationService("ConnectionString", "false");
            notificationService.AddIssue("A new issue");

            Assert.IsNull(notificationService.FetchIssues());
        }

        [Test]
        public void FetchErrors_LowerCaseLoggingDisabledAddIssue_ReturnsNull()
        {
            notificationService = new NotificationService("ConnectionString", "FALSE");
            notificationService.AddIssue("A new issue");

            Assert.IsNull(notificationService.FetchIssues());
        }

        [Test]
        public void FetchErrors_LoggingEnabledAddIssueFetchIssues_ReturnsIssue()
        {
            notificationService = new NotificationService("ConnectionString", "true");
            notificationService.AddIssue("A new issue");
            Assert.AreEqual(new[] { "A new issue" }, notificationService.FetchIssues());
        }

        [Test]
        public void FetchErrors_LoggingEnabledAddIssueFetchErrors_ReturnsNull()
        {
            notificationService = new NotificationService("ConnectionString", "true");
            notificationService.AddIssue("A new error");
            Assert.IsNull(notificationService.FetchErrors());
        }

        [Test]
        public void FetchErrors_LoggingEnabledAddErrorFetchIssues_ReturnsNull()
        {
            notificationService = new NotificationService("ConnectionString", "true");
            notificationService.AddError("A new error");
            Assert.IsNull(notificationService.FetchIssues());
        }

        [Test]
        public void FetchErrors_LoggingEnabledAddTwoIssuesFetchIssues_ReturnsTwoIssues()
        {
            notificationService = new NotificationService("ConnectionString", "true");
            notificationService.AddIssue("A new issue");
            notificationService.AddIssue("A new issue2");
            Assert.AreEqual(2, notificationService.FetchIssues().Count());
        }

        [Test]
        public void FetchErrors_LoggingEnabledAddTwoErrorsFetchErrors_ReturnsTwoErrors()
        {
            notificationService = new NotificationService("ConnectionString", "true");
            notificationService.AddError("A new error");
            notificationService.AddError("A new error2");
            Assert.AreEqual(2, notificationService.FetchErrors().Count());
        }
        
        [TearDown]
        public void TearDown()
        {
            notificationService = null;
        }
    }
}

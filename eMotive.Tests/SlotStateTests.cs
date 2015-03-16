using eMotive.Models.Objects.Signups;
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
    public class SlotStateTests
    {
        private SlotState slotState;

        [SetUp]
        public void Setup()
        {
            slotState = new SlotState();
        }

        [Test]
        public void PlacesAvailible_NoPlacesLeft_Returns0()
        {
            slotState.TotalPlacesAvailable = 8;
            slotState.TotalReserveAvailable = 2;
            slotState.TotalInterestedAvaiable = 5;

            slotState.NumberSignedUp = 8;

            Assert.AreEqual(0, slotState.PlacesAvailable());
        }

        [TearDown]
        public void TearDown()
        {
            slotState = null;
        }
    }
}

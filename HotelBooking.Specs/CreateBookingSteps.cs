using HotelBooking.Core;
using Moq;
using Reqnroll;


namespace HotelBooking.Specs
{
    [Binding]
    public class CreateBookingSteps
    {
        private readonly IBookingManager _bookingManager;
        private readonly Mock<IRepository<Booking>> _bookingRepoMock;
        private readonly Mock<IRepository<Room>> _roomRepoMock;
        private Booking _booking;
        private bool _result;
        private Exception _exception;

        public CreateBookingSteps()
        {
            _bookingRepoMock = new Mock<IRepository<Booking>>();
            _roomRepoMock = new Mock<IRepository<Room>>();
            _bookingManager = new BookingManager(_bookingRepoMock.Object, _roomRepoMock.Object);
        }

        [Given(@"the current date is today")]
        public void GivenTheCurrentDateIsToday()
        {
            // No action needed since BookingManager uses DateTime.Today by default
            Console.WriteLine($"Current date is: {DateTime.Today:yyyy-MM-dd}");
        }

        [Given(@"there is an available room")]
        public void GivenThereIsAnAvailableRoom()
        {
            var rooms = new List<Room> { new Room { Id = 1, Description = "Room 1" } };
            _roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            _bookingRepoMock.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>());
        }

        [Given(@"all rooms are booked from tomorrow to two days from now")]
        public void GivenAllRoomsAreBookedFromTomorrowToTwoDaysFromNow()
        {
            var rooms = new List<Room> { new Room { Id = 1, Description = "Room 1" } };
            var bookings = new List<Booking>
            {
                new Booking
                {
                    RoomId = 1,
                    StartDate = DateTime.Today.AddDays(1), // Tomorrow
                    EndDate = DateTime.Today.AddDays(2),   // Two days from now
                    IsActive = true
                }
            };
            _roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            _bookingRepoMock.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);
        }

        [When(@"I create a booking with start date tomorrow and end date two days from now")]
        public async Task WhenICreateABookingTomorrowToTwoDaysFromNow()
        {
            _booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(1), // Tomorrow
                EndDate = DateTime.Today.AddDays(2),   // Two days from now
                CustomerId = 1
            };
            try
            {
                _result = await _bookingManager.CreateBooking(_booking);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [When(@"I create a booking with start date yesterday and end date tomorrow")]
        public async Task WhenICreateABookingYesterdayToTomorrow()
        {
            _booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(-1), // Yesterday
                EndDate = DateTime.Today.AddDays(1),    // Tomorrow
                CustomerId = 1
            };
            try
            {
                _result = await _bookingManager.CreateBooking(_booking);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [When(@"I create a booking with start date tomorrow and end date today")]
        public async Task WhenICreateABookingTomorrowToToday()
        {
            _booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(1), // Tomorrow
                EndDate = DateTime.Today,              // Today
                CustomerId = 1
            };
            try
            {
                _result = await _bookingManager.CreateBooking(_booking);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
        }

        [Then(@"the booking should be created successfully")]
        public void ThenTheBookingShouldBeCreatedSuccessfully()
        {
            Assert.IsTrue(_result, "Booking should have been created.");
            Assert.IsNull(_exception, "No exception should have been thrown.");
            _bookingRepoMock.Verify(b => b.AddAsync(It.Is<Booking>(bk => bk.RoomId == 1 && bk.IsActive)), Times.Once());
        }

        [Then(@"an error should be thrown with message ""(.*)""")]
        public void ThenAnErrorShouldBeThrown(string message)
        {
            Assert.IsNotNull(_exception, "An exception should have been thrown.");
            Assert.AreEqual(message, _exception.Message);
        }

        [Then(@"the booking should not be created")]
        public void ThenTheBookingShouldNotBeCreated()
        {
            Assert.IsFalse(_result, "Booking should not have been created.");
            Assert.IsNull(_exception, "No exception should have been thrown.");
            _bookingRepoMock.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Never());
        }
    }
}
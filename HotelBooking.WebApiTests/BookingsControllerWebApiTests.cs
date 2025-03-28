using HotelBooking.Core;
using HotelBooking.WebApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;


namespace HotelBooking.ApiTests
{
    [TestClass]
    public sealed class BookingsControllerWebApiTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private Mock<IRepository<Booking>> _bookingRepoMock;
        private Mock<IRepository<Room>> _roomRepoMock;

        [TestInitialize]
        public void Setup()
        {
            _bookingRepoMock = new Mock<IRepository<Booking>>();
            _roomRepoMock = new Mock<IRepository<Room>>();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddScoped(_ => _bookingRepoMock.Object);
                        services.AddScoped(_ => _roomRepoMock.Object);
                    });
                });

            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [TestMethod]
        public async Task CreateBooking_Success_Returns201()
        {
            // Arrange
            _roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 1, Description = "Room 1" } });
            _bookingRepoMock.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>());
            _bookingRepoMock.Setup(b => b.AddAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);

            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(1), // Tomorrow
                EndDate = DateTime.Today.AddDays(2),   // Two days from now
                CustomerId = 1
            };
            var content = new StringContent(JsonSerializer.Serialize(booking), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/bookings", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(await response.Content.ReadAsStringAsync()));
        }

        [TestMethod]
        public async Task CreateBooking_PastStartDate_Returns500()
        {
            // Arrange
            _roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 1, Description = "Room 1" } });
            _bookingRepoMock.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>());

            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(-1), // Yesterday
                EndDate = DateTime.Today.AddDays(1),    // Tomorrow
                CustomerId = 1
            };
            var content = new StringContent(JsonSerializer.Serialize(booking), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/bookings", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("The start date cannot be in the past or later than the end date."));
        }

        [TestMethod]
        public async Task CreateBooking_InvalidDateRange_Returns500()
        {
            // Arrange
            _roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 1, Description = "Room 1" } });
            _bookingRepoMock.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>());

            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(1), // Tomorrow
                EndDate = DateTime.Today,              // Today
                CustomerId = 1
            };
            var content = new StringContent(JsonSerializer.Serialize(booking), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/bookings", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("The start date cannot be in the past or later than the end date."));
        }

        [TestMethod]
        public async Task CreateBooking_NoRoomsAvailable_Returns409()
        {
            // Arrange
            _roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 1, Description = "Room 1" } });
            _bookingRepoMock.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>
            {
                new Booking
                {
                    RoomId = 1,
                    StartDate = DateTime.Today.AddDays(1), // Tomorrow
                    EndDate = DateTime.Today.AddDays(2),   // Two days from now
                    IsActive = true
                }
            });

            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(1), // Tomorrow
                EndDate = DateTime.Today.AddDays(2),   // Two days from now
                CustomerId = 1
            };
            var content = new StringContent(JsonSerializer.Serialize(booking), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/bookings", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("The booking could not be created. All rooms are occupied."));
        }

        
    }
}
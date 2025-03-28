using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using Moq;



namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        
        private readonly Mock<IRepository<Booking>> mockBookingRepository;
        private readonly Mock<IRepository<Room>> mockRoomRepository;
        private readonly BookingManager bookingManager;


        public BookingManagerTests(){
            mockBookingRepository = new Mock<IRepository<Booking>>();
            mockRoomRepository = new Mock<IRepository<Room>>();
            bookingManager = new BookingManager(mockBookingRepository.Object, mockRoomRepository.Object);

        }

        [Fact]
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Task Result() =>  bookingManager.FindAvailableRoom(date, date);
         
            // Assert
            await Assert.ThrowsAsync<ArgumentException>(Result);
        }
        
        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            mockBookingRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Booking>());
            mockRoomRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 321 } });
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            
            // Assert
            Assert.NotEqual(-1, roomId);
        }
        
        //Data-driven 
        [Theory]
        [InlineData("2025-07-12", "2025-07-16")]
        [InlineData("2025-08-13", "2025-08-17")]
        [InlineData("2025-09-14", "2025-09-18")]
        [InlineData("2025-10-15", "2025-10-19")]
        [InlineData("2025-11-16", "2025-11-20")]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom(string startDateStr, string endDateStr)
        {
            // Arrange
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);
            mockBookingRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Booking>());
            mockRoomRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 123 } });

            // Act
            var result = await bookingManager.FindAvailableRoom(startDate, endDate);
            
            
            // Assert
            Assert.Equal(123, result);
        }
        
        [Fact]
        public async Task CreateBooking_RoomAvailable_ReturnsTrue()
        {
            // Arrange
            var booking = new Booking { StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(3) };
            mockBookingRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Booking>());
            mockRoomRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 1} });

            // Act
            var result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateBooking_NoRoomAvailable_ReturnsFalse()
        {
            // Arrange
            var booking = new Booking { StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(3) };
            mockBookingRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Booking> { new Booking { RoomId = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(3), IsActive = true } });
            mockRoomRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 1 } });

            // Act
            var result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(result);
        }
        
        
        [Fact]
        public async Task FindAvailableRoom_NoRoomAvailable_ReturnsNegativeOne()
        {
            // Arrange
            mockBookingRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Booking> { new Booking { RoomId = 231, StartDate = DateTime.Today.AddDays(6), EndDate = DateTime.Today.AddDays(9), IsActive = true } });
            mockRoomRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Room> { new Room { Id = 231 } });

            // Act
            int result = await bookingManager.FindAvailableRoom(DateTime.Today.AddDays(6), DateTime.Today.AddDays(9));
            
            // Assert
            Assert.Equal(-1, result);
        }
        
        //Data-driven 
        [Theory]
        [InlineData("2025-07-12", "2025-07-08")]
        [InlineData("2025-08-13", "2025-08-09")]
        [InlineData("2025-09-14", "2025-09-10")]
        [InlineData("2025-10-15", "2025-10-11")]
        [InlineData("2025-11-16", "2025-11-12")]
        public async Task FindAvailableRoom_StartDateAfterEndDate_ThrowsArgumentException(string startDateStr, string endDateStr)
        {
            // Arrange
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

            // Act
            Task Result() =>  bookingManager.FindAvailableRoom(startDate, endDate);
         
            // Assert
            await Assert.ThrowsAsync<ArgumentException>(Result);
        }


        [Fact]
        public async Task GetFullyOccupiedDates_AllRoomsBooked_ReturnsOccupiedDates()
        {
            // Arrange
            var rooms = new List<Room> { new Room { Id = 1 }, new Room { Id = 2 } };
            mockRoomRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(rooms);
            mockBookingRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Booking> {
                new Booking { RoomId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5), IsActive = true },
                new Booking { RoomId = 2, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5), IsActive = true }
            });

            //Act
            var result = await bookingManager.GetFullyOccupiedDates(DateTime.Today, DateTime.Today.AddDays(5));

            // Assert
            Assert.Equal(6, result.Count);
        }
        
        //Data-driven 
        [Theory]
        [InlineData("2025-07-12", "2025-07-08")]
        [InlineData("2025-08-13", "2025-08-09")]
        [InlineData("2025-09-14", "2025-09-10")]
        [InlineData("2025-10-15", "2025-10-11")]
        [InlineData("2025-11-16", "2025-11-12")]
        public async Task GetFullyOccupiedDates_StartDateAfterEndDate_ThrowsArgumentException(string startDateStr, string endDateStr)
        {
            // Arrange
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

            // Act
            Task Result() =>  bookingManager.GetFullyOccupiedDates(startDate, endDate);
         
            // Assert
            await Assert.ThrowsAsync<ArgumentException>(Result);
        }


        
       
        
        
        
        

    }
}

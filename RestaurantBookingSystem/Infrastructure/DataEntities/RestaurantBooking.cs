using System;
using System.Collections.Generic;
using RestaurantBookingSystem.Helpers;

namespace RestaurantBookingSystem.Infrastructure.DataEntities
{
    /// <summary>
    /// A class to represent a Booking in the Restaurant
    /// </summary>
    public class RestaurantBooking
    {
        /// <summary>
        /// Gets the Id for the Booking
        /// </summary>
        public int BookingId { get; private set; }

        /// <summary>
        /// Gets the Instance of the Booking Customer
        /// </summary>
        public UserBase BookingCustomer { get; set; }

        /// <summary>
        /// Gets or Sets the DateTime of the time this booking was made
        /// </summary>
        public DateTime BookedOn { get; set; }

        /// <summary>
        /// Gets or Sets the DateTime for which this booking is made.
        /// </summary>
        public DateTime BookedFor { get; set; }

        /// <summary>
        /// Gets or Sets the DateTime for till which this booking is valid
        /// </summary>
        public DateTime BookedTill { get; set; }

        /// <summary>
        /// Gets or Sets the List of Tables Booked
        /// </summary>
        public IList<RestaurantTable> BookedTables { get; set; }

        /// <summary>
        /// Gets or Sets the List of Menu Items Preffered
        /// </summary>
        public IList<RestaurantMenuItem> PrefferedMenuItems { get; set; }

        /// <summary>
        /// Gets or Sets the Bill for the Booking
        /// </summary>
        public IList<BookingBill> Bills { get; set; }

        /// <summary>
        /// Gets or Sets the Status of the Booking
        /// </summary>
        public BookingStatus Status { get; set; }

        /// <summary>
        /// A constructor to initialize a Booking Instance
        /// </summary>
        public RestaurantBooking()
            : this(0)
        {
        }

        /// <summary>
        /// A constructor to initialize a Booking Instance with a given Id
        /// </summary>
        /// <param name="bookingid">A Booking Id</param>
        public RestaurantBooking(int bookingid)
        {
            BookingId = bookingid;
            BookedOn = DateTimeHelper.SqlDbMinDateTime;
            BookedFor = DateTimeHelper.SqlDbMinDateTime;
            BookedTill = DateTimeHelper.SqlDbMinDateTime;
            BookedTables = new List<RestaurantTable>();
            PrefferedMenuItems = new List<RestaurantMenuItem>();
            Bills = new List<BookingBill>();
        }

        /// <summary>
        /// Enumeration defines different status of the booking.
        /// </summary>
        public enum BookingStatus
        {
            InProcess = 0,
            Confirmed = 1,
            Served = 2,
            Cancelled = 3
        }
    }
}
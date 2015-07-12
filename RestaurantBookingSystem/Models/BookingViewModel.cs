using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;

namespace RestaurantBookingSystem.Models
{
    /// <summary>
    /// A View Model class to represent a Booking
    /// </summary>
    [Serializable]
    public class BookingViewModel
    {
        /// <summary>
        /// Gets or Sets the DateTime for which this booking is made.
        /// </summary>
        [Required]
        public DateTime BookedFor { get; set; }

        /// <summary>
        /// Gets or Sets the DateTime for till which this booking is valid
        /// </summary>
        [Required]
        public int BookedSlots { get; set; }

        /// <summary>
        /// Gets or Sets the CSV of Tables Booked
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "At lest one table needs to be selected for Booking", MinimumLength = 1)]
        public string BookedTables { get; set; }

        /// <summary>
        /// Gets or Sets the CSV of Menu Items Preffered
        /// </summary>
        public string PrefferedMenuItems { get; set; }

        public BookingViewModel()
        {
            BookedFor = DateTimeHelper.SqlDbMinDateTime;
        }
    }

    public class ConfirmBookingViewModel : BookingViewModel
    {
        public List<RestaurantTable> Tables { get; set; }
        public List<RestaurantMenuItem> MenuItems{ get; set; }
        public BookingBill Bill{ get; set; }
        public SeasonalOffer Offer{ get; set; }
    }
}
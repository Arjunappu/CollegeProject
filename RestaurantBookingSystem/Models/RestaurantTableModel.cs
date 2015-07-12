using System.ComponentModel.DataAnnotations;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;

namespace RestaurantBookingSystem.Models
{
    public class RestaurantTableModel
    {
        /// <summary>
        /// Position of the table in the Restaurant (X,Y defines the grid in the Restaurant Floor Plan)
        /// </summary>
        [Required]
        [Digit( ErrorMessage = "A Pixel Position can only be numeric")]
        [Display(Name = "Position X")]
        public int PositionX { get; set; }

        /// <summary>
        /// Position of the table in the Restaurant (X,Y defines the grid in the Restaurant Floor Plan)
        /// </summary>
        [Required]
        [Digit(ErrorMessage = "A Pixel Position can only be numeric")]
        [Display(Name = "Position Y")]
        public int PositionY { get; set; }

        /// <summary>
        /// The Type of the Table defined by Enum <see cref="RestaurantTable.RestaurantTableType"/>
        /// </summary>
        [Required]
        [UIHint("JuiRadios")]
        [Display(Name = "Table Seating Capacity")]
        public RestaurantTable.RestaurantTableType TableType { get; set; }

        /// <summary>
        /// Alingment of the Table in Restaurant defined by Enum <see cref="RestaurantTable.RestaurantTableAlignment"/>
        /// </summary>
        [Required]
        [UIHint("JuiRadios")]
        [Display(Name = "Table Alingment")]
        public RestaurantTable.RestaurantTableAlignment Alignment { get; set; }

        /// <summary>
        /// The price of booking the table
        /// </summary>
        [Required]
        [Display(Name = "Table Booking Price")]        
        public int Price { get; set; }

        /// <summary>
        /// The File Name of the Floor Plan to which this table belongs
        /// </summary>
        [Required]
        [Display(Name = "Floor Plan")]
        public string FloorPlanFileName { get; set; }

        public RestaurantTableModel()
        {
            TableType = RestaurantTable.RestaurantTableType.Dual;
            Alignment = RestaurantTable.RestaurantTableAlignment.Horizontal;
        }
    }
}
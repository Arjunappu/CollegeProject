using System.Drawing;

namespace RestaurantBookingSystem.Infrastructure.DataEntities
{
    /// <summary>
    /// A class that represents a Table in a Restaurant
    /// </summary>
    public class RestaurantTable
    {
        /// <summary>
        /// The Table Id
        /// </summary>
        public int TableId { get; private set; }

        /// <summary>
        /// Position of the table in the Restaurant (X,Y defines the grid in the Restaurant Floor Plan)
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// The Type of the Table defined by Enum <see cref="RestaurantTableType"/>
        /// </summary>
        public RestaurantTableType TableType { get; set; }

        /// <summary>
        /// Alingment of the Table in Restaurant defined by Enum <see cref="RestaurantTableAlignment"/>
        /// </summary>
        public RestaurantTableAlignment Alignment { get; set; }

        /// <summary>
        /// Current status of the table defined by Enum <see cref="RestaurentTableStatus"/>
        /// </summary>
        public RestaurentTableStatus Status { get; set; }

        /// <summary>
        /// The price of booking the table
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// The Name of the Floor Plan File to which the current instance of Table belongs to
        /// </summary>
        public string FloorPlanFileName { get; set; }

        /// <summary>
        /// The type of the table according to its size and capacity
        /// </summary>
        public enum RestaurantTableType
        {
            Dual = 2,
            Quad = 4,
            Hex = 6,
            Oct = 8
        }

        /// <summary>
        /// Horizontal or Vertical Alingment of the Table in Restaurant
        /// </summary>
        public enum RestaurantTableAlignment
        {
            Horizontal,
            Vertical
        }

        /// <summary>
        /// Status Enumeration of the table
        /// </summary>
        public enum RestaurentTableStatus
        {
            //TODO: Fix this NA Status type and think of any other way to diff. b/w Table with Time context and one without
            /// <summary> 
            /// This Table status id for Table instances which are not retrived for a time context
            /// </summary>
            NotApplicable = -1,
            Booked = 0,
            Occupied = 1,
            Vacant = 2
        }

        /// <summary>
        /// A constructor to initialize a new Table
        /// </summary>
        public RestaurantTable()
            : this(0)
        {
        }

        /// <summary>
        /// A constructor to initialize a new Table with a given table id
        /// </summary>
        /// <param name="tableId">The ID for the table</param>
        public RestaurantTable(int tableId)
        {
            TableId = tableId;
        }
    }

}
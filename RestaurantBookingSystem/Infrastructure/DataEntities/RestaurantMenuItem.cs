using System;

namespace RestaurantBookingSystem.Infrastructure.DataEntities
{
    /// <summary>
    /// An class to represent a MenuItem which can be served in a Restaurent
    /// </summary>
    public class RestaurantMenuItem
    {
        /// <summary>
        /// Gets The Item Id
        /// </summary>
        public int ItemId { get; private set; }

        /// <summary>
        /// Gets or Sets the File name of the Picture for the Menu Item
        /// </summary>
        public string PictureFile { get; set; }

        /// <summary>
        /// Gets or Sets a Name of the Menu Item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets  A Description of the Menu Item
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets the Price of the Menu Item
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Constructor to Initialize a Restaurant Menu Item
        /// </summary>
        public RestaurantMenuItem()
            : this(0)
        {
        }

        /// <summary>
        /// Constructor to Initialize a Restaurant Menu Item with given Item Id
        /// </summary>
        /// <param name="itemid">Id for the Menu Item</param>
        public RestaurantMenuItem(int itemid)
        {
            ItemId = itemid;
        }
    }

    /// <summary>
    /// A class to represent a MenuItem which has been booked by a customer
    /// </summary>
    public class BookedRestaurantMenuItem : RestaurantMenuItem
    {
        /// <summary>
        /// Gets or Sets the Quantity of the Booked Item
        /// </summary>
        public int Qty { get; set; }

        /// <summary>
        /// Gets or Sets the Date for which the booking was done
        /// </summary>
        public DateTime BookedFor { get; set; }

        /// <summary>
        /// Constructor to initialize a Menu Item with given Item Id
        /// </summary>
        /// <param name="id"></param>
        public BookedRestaurantMenuItem(int id):base(id){}
    }
}
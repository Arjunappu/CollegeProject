using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace RestaurantBookingSystem.Infrastructure.DataEntities
{

    /// <summary>
    /// A class to represent a Bill
    /// </summary>
    public class BookingBill
    {
        /// <summary>
        /// Gets the Id of the Bill
        /// </summary>
        [HiddenInput]
        [Display(Name = "Bill No")]
        public int BillId { get; private set; }
        
        /// <summary>
        /// Gets or Sets the gross total amount of the bill
        /// </summary>
        [HiddenInput]
        [Display(Name = "Gross Amount")]
        [DisplayFormat(DataFormatString = "Rs: #.00 /-")]
        public decimal GrossAmount { get; set; }

        /// <summary>
        /// Gets or Sets the net total amount actually charged for the bill
        /// </summary>
        [HiddenInput]
        [Display(Name = "Amount Payable")]
        [DisplayFormat(DataFormatString = "Rs: #.00 /-")]
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Gets or Sets the Amounted discounted to the customer
        /// </summary>
        [HiddenInput]
        [Display(Name = "Discount Amount")]
        [DisplayFormat(DataFormatString = "Rs: #.00 /-", NullDisplayText = "None")]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets the booking id associated with the Bill
        /// </summary>
        [HiddenInput]
        [Display(Name = "Booking No")]
        public int BookingId { get; private set; }


        /// <summary>
        /// Constructor to initialize an instance of a bill
        /// </summary>
        public BookingBill()
            : this(0)
        {
        }

        /// <summary>
        /// Constructor to initialize an instance of a bill with given BookingId
        /// </summary>
        /// <param name="bookingid">BookingId for which the bill is generated</param>
        public BookingBill(int bookingid)
            : this(0, bookingid)
        {
        }

        /// <summary>
        /// Constructor to initialize an instance of a bill with given BillID and BookingId
        /// </summary>
        /// <param name="billid">Bill Id of the Bill</param>
        /// <param name="bookingid">BookingId for which the bill is generated</param>
        public BookingBill(int billid, int bookingid)
        {
            BillId = billid;
            BookingId = bookingid;
        }
    }
}
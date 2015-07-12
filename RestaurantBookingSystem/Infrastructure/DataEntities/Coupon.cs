namespace RestaurantBookingSystem.Infrastructure.DataEntities
{
    /// <summary>
    /// A class that represents an offer of Coupon type
    /// </summary>
    public class Coupon : OfferBase
    {
        /// <summary>
        /// Constructor to Initialize a Coupon with Unique CouponCode
        /// </summary>
        /// <param name="couponcode">Unique code of the coupon</param>
        public Coupon(string couponcode)
            : this(0, couponcode)
        {
        }

        /// <summary>
        /// Constructor to Initialize a Coupon with OfferId and a Unique CouponCode
        /// </summary>
        /// <param name="offerid">Offer Id of the coupon</param>
        /// <param name="couponcode">Unique code of the coupon</param>
        public Coupon(int offerid, string couponcode)
            : base(offerid)
        {
            Code = couponcode;
        }

        /// <summary>
        /// Unique Coupon code of the offer
        /// </summary>
        public string Code { get; private set; }
    }
}
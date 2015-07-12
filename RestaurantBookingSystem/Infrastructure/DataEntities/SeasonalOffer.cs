namespace RestaurantBookingSystem.Infrastructure.DataEntities
{
    /// <summary>
    /// A class that represents a offer
    /// </summary>
    public class SeasonalOffer : OfferBase
    {
        /// <summary>
        /// Constructor to Initialize an offer
        /// </summary>
        public SeasonalOffer()
            : this(0)
        {
        }

        /// <summary>
        /// Constructor to Initialize an offer with a given OfferId
        /// </summary>
        /// <param name="offerid">UniqueId for the offer</param>
        public SeasonalOffer(int offerid)
            : base(offerid)
        {
        }


        /// <summary>
        /// Picture associated with the offer
        /// </summary>
        /// <remarks>This will be shown in the Offers Details</remarks>
        public string PictureFileName { get; set; }
    }
}
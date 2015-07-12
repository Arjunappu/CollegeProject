using System;
using RestaurantBookingSystem.Helpers;

namespace RestaurantBookingSystem.Infrastructure.DataEntities
{
    /// <summary>
    /// An abstract base class for the different offers in a restaurant.
    /// </summary>
    public abstract class OfferBase
    {
        /// <summary>
        /// Unique Id of the SeasonalOffer
        /// </summary>
        public int OfferId { get; private set; }

        /// <summary>
        /// Value of the SeasonalOffer. Use <see cref="OfferType"/> Enumeration to define type.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Type of the offer. One of the <see cref="OfferType"/> Enumeration
        /// </summary>
        public OfferType Type { get; set; }

        /// <summary>
        /// This defines the type for the <see cref="OfferBase.Value"/> of the offer
        /// </summary>
        public enum OfferType
        {
            /// <summary>
            /// If the Type of offer is DiscountAmount then <see cref="OfferBase.Value"/> is the absolute Amount of Discount
            /// </summary>
            DiscountAmount,

            /// <summary>
            /// If the Type of offer is DiscountPercent then <see cref="OfferBase.Value"/> is the Percent of Amount Discounted
            /// </summary>
            DiscountPercent,

            /// <summary>
            /// If the Type of offer is FreeServing then <see cref="OfferBase.Value"/> is MenuItemID that will be provided free of cost.
            /// </summary>
            FreeServing
        }

        /// <summary>
        /// A title for the offer
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A Short Description of the offer
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The date from which offer becomes valid
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Th date on which offer expires
        /// </summary>
        public DateTime ValidTill { get; set; }

        /// <summary>
        /// Constructor to Initialize an offer
        /// </summary>
        protected OfferBase()
            : this(0)
        {
        }

        /// <summary>
        /// Constructor to Initialize an offer with a given OfferId
        /// </summary>
        /// <param name="offerid">UniqueID for the offer</param>
        protected OfferBase(int offerid)
        {
            OfferId = offerid;
            ValidFrom = DateTimeHelper.SqlDbMinDateTime;
            ValidTill = DateTimeHelper.SqlDbMinDateTime;
        }
    }
}
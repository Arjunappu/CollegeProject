using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using RestaurantBookingSystem.Infrastructure.DataEntities;

namespace RestaurantBookingSystem.Models
{
    public class AddOfferModel
    {
        /// <summary>
        /// A title for the offer
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        public string Title { get; set; }

        /// <summary>
        /// A Short Description of the offer
        /// </summary>
        [Required]
        [Display(Description = "A short description of the offer")]
        [DataType(DataType.MultilineText)]
        [StringLength(100)]
        public string Description { get; set; }

        /// <summary>
        /// The date from which offer becomes valid
        /// </summary>
        [Required]
        [UIHint("JuiCalander")]
        [DataType(DataType.Date)]
        [AdditionalMetadata("JuiMinDate", "0D")] // Today
        [AdditionalMetadata("JuiMaxDate", "+3M")] //3 Months (see Jui Documentation for Calander control)
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// The date on which offer expires
        /// </summary>
        [Required]
        [UIHint("JuiCalander")]
        [DataType(DataType.Date)]
        [AdditionalMetadata("JuiMinDate", "+1D")] // Tommorow
        [AdditionalMetadata("JuiMaxDate", "+3M")] //3 Months (see Jui Documentation for Calander control)
        public DateTime ValidTill { get; set; }

        /// <summary>
        /// Value of the SeasonalOffer. Use <see cref="OfferBase.OfferType"/> Enumeration to define type.
        /// </summary>
        [Display(Name = "Offer Value", Description = "This sets the value of the offer type, like if offer type is Discount Amount then Value of 20 will define Discount of Rs. 20")]
        [Required(ErrorMessage = "Value of an Offer is Required")]
        [Range(0.1D, Double.MaxValue, ErrorMessage = "Value of an Offer cannot be less than 0.1")]
        public int Value { get; set; }

        /// <summary>
        /// Type of the offer. One of the <see cref="OfferBase.OfferType"/> Enumeration
        /// </summary>
        [UIHint("JuiRadios")]
        [Display(Name = "Offer Type", Description = "It defines the type of the offer and is associated with the value of the offer")]
        public OfferBase.OfferType Type { get; set; }

        /// <summary>
        /// Type of the offer. One of the <see cref="OfferBase.OfferType"/> Enumeration
        /// </summary>
        [UIHint("JuiRadios")]
        [Display(Name = "Published Via", Description = "It defines the publish media of the offer, it defines the way in which it will be used")]
        public PublishedVia PublishedBy { get; set; }

        private string _couponCode;
        /// <summary>
        /// A title for the offer
        /// </summary>
        [Display(Name = "Coupon Code")]
        [StringLength(20, ErrorMessage = "A Coupon code can only have 5 to 20 characters")]
        public string CouponCode
        {
            get { return _couponCode; }
            set { _couponCode = value != null ? value.ToUpperInvariant() : null; }
        }

        public AddOfferModel()
        {
            ValidFrom = DateTime.UtcNow.Date;
            ValidTill = DateTime.UtcNow.Date.AddDays(3);
        }

        public enum PublishedVia
        {
            Code,
            Advertisement
        }
    }

    public class UpdateOfferModel
    {
        /// <summary>
        /// A title for the offer
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        public string Title { get; set; }

        /// <summary>
        /// A Short Description of the offer
        /// </summary>
        [Required]
        [Display(Description = "A short description of the offer")]
        [DataType(DataType.MultilineText)]
        [StringLength(100)]
        public string Description { get; set; }

        /// <summary>
        /// The date on which offer expires
        /// </summary>
        [Required]
        [UIHint("JuiCalander")]
        [DataType(DataType.Date)]
        [AdditionalMetadata("JuiMinDate", "0D")] // Today
        [AdditionalMetadata("JuiMaxDate", "+3M")] //3 Months (see Jui Documentation for Calander control)
        public DateTime ValidTill { get; set; }

        /// <summary>
        /// Type of the offer. One of the <see cref="OfferBase.OfferType"/> Enumeration
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Published Via", Description = "It defines the publish media of the offer, it defines the way in which it will be used")]
        public PublishedVia PublishedBy { get; set; }

        public UpdateOfferModel()
        {}

        public UpdateOfferModel(OfferBase offer)
        {
            ValidTill = offer.ValidTill;
            Title = offer.Title;
            Description = offer.Description;
            PublishedBy = offer is Coupon ? PublishedVia.Code : PublishedVia.Advertisement;
        }

        public enum PublishedVia
        {
            Code,
            Advertisement
        }
    }
}

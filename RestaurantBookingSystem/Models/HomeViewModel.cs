using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Repositories;


namespace RestaurantBookingSystem.Models
{
    /// <summary>
    /// A Model class specifically for Home View
    /// </summary>
    public class HomeViewModel
    {
        /// <summary>
        /// Stores a list of Advertisements to show on the hiome page.
        /// </summary>
        public readonly IEnumerable<SeasonalOffer> AdvertisementsList;

        /// <summary>
        /// Stores a list of new Restaurant menu items to show on home page.
        /// </summary>
        public readonly IEnumerable<RestaurantMenuItem> RestaurantMenuItems;

        /// <summary>
        /// Constructor to initialize AdvertisementList and RestaurantMenuItems lists.
        /// </summary>
        public HomeViewModel()
        {
            AdvertisementsList = new OfferBaseRepository().GetAllSeasonalOffers().Where( offer => offer.ValidTill > DateTime.Now);
            RestaurantMenuItems = new RestaurantMenuItemRepository().GetNewMenuItems(5).ToList();
        }
    }
}
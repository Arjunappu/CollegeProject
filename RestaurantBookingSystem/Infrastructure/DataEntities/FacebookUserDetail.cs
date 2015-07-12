using System;
using RestaurantBookingSystem.Helpers;

namespace RestaurantBookingSystem.Infrastructure.DataEntities
{
    public class FacebookUserDetail
    {
        /// <summary>
        /// Gets or Sets The Facebook User Id or FbId
        /// </summary>
        public UInt64 FacebookId { get; set; }

        /// <summary>
        /// Gets or Sets The OAuth Token which can be used for accessing facebook user data
        /// </summary>
        public string OAuthToken { get; set; }

        /// <summary>
        /// Gets wheather OAuth Token has Expired
        /// </summary>
        public bool TokenExpired
        {
            get { return ExpiresOn.ToUniversalTime() < DateTime.UtcNow; }
        }

        /// <summary>
        /// Gets or Sets The DateTime of the OAuthToken Expiry
        /// </summary>
        public DateTime ExpiresOn { get; set; }

        /// <summary>
        /// Gets orSets The User's Facebook Profile Link Uri
        /// </summary>
        public Uri ProfileLink { get; set; }

        public FacebookUserDetail()
        {
            ExpiresOn = DateTimeHelper.SqlDbMinDateTime;
        }
    }
}